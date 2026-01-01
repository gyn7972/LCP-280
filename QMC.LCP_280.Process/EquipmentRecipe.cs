using QMC.Common;
using QMC.Common.Component;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit;
using System;
using System.IO;

namespace QMC.LCP_280.Process
{
    public class EquipmentRecipe
    {
        // ===== Global MeasurementRecipe System =====
        #region Global MeasurementRecipe

        public MeasurementRecipe CurrentRecipe { get; set; }
        public string CurrentRecipeName => CurrentRecipe?.Name ?? _currentRecipeNameFallback;
        public static event EventHandler<MeasurementRecipeChangedEventArgs> CurrentRecipeChanged;

        private static readonly object _recipeLock = new object();
        private static string _currentRecipeNameFallback = "Default";

        // ===== [ADD] UnitRecipe 캐시 =====
        public IndexChipProbeControllerRecipe IndexChipProbeControllerTeachingRecipe { get; private set; }
        
        // [ADD] MeasurementRecipe 기준 UnitRecipeName 정규화
        internal string GetOrLoadIndexChipProbeControllerTeachingRecipeName()
        {
            if (CurrentRecipe == null)
                return "Default_ProbeTeaching";

            EnsureUnitRecipeNamesBoundToCurrentRecipe();

            var tpName = CurrentRecipe.IndexChipProbeControllerTeachingRecipeName;
            if (string.IsNullOrWhiteSpace(tpName))
                tpName = GetExpectedProbeTeachingName();

            return tpName;
        }

        // [ADD] Config 쪽에서 로드한 UnitRecipe 인스턴스를 캐시에 주입(단일화)
        internal void SetIndexChipProbeControllerTeachingRecipe(IndexChipProbeControllerRecipe recipe)
        {
            if (recipe == null) 
                return;

            // MeasurementRecipe가 참조하는 이름으로 강제 동기화(파일 경로 규약 안정화)
            var expectedName = GetOrLoadIndexChipProbeControllerTeachingRecipeName();
            if (!string.IsNullOrWhiteSpace(expectedName) &&
                !string.Equals(recipe.Name, expectedName, StringComparison.OrdinalIgnoreCase))
            {
                recipe.Name = expectedName;
            }

            IndexChipProbeControllerTeachingRecipe = recipe;
        }

        // 초기화: InitializeEquipment() 안에서 호출
        public void InitGlobalRecipe()
        {
            try
            {
                // EquipmentConfig 먼저 확보
                var eq = Equipment.Instance;
                eq.EquipmentConfig = EquipmentConfig.LoadOrCreate(); // 정적 호출로 고정
                var name = eq.EquipmentConfig?.CurrentRecipeName;
                if (string.IsNullOrWhiteSpace(name))
                    name = _currentRecipeNameFallback;

                LoadRecipeInternal(name, raiseEvent: false);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                lock (_recipeLock)
                {
                    CurrentRecipe = new MeasurementRecipe(_currentRecipeNameFallback);
                    CurrentRecipe.Reset();
                    SafeSaveRecipeNoThrow(CurrentRecipe);
                }
            }
            CurrentRecipeChanged?.Invoke(null, new MeasurementRecipeChangedEventArgs(CurrentRecipe));
        }

        // 외부 사용: 레시피 얻기 (null 방지)
        public MeasurementRecipe GetRecipe()
        {
            lock (_recipeLock)
            {
                if (CurrentRecipe == null)
                {
                    CurrentRecipe = new MeasurementRecipe(_currentRecipeNameFallback);
                    CurrentRecipe.Reset();
                    SafeSaveRecipeNoThrow(CurrentRecipe);
                }
                return CurrentRecipe;
            }
        }

        // 이름으로 로드 (없으면 생성)
        public MeasurementRecipe LoadRecipe(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) name = _currentRecipeNameFallback;
            return LoadRecipeInternal(name, raiseEvent: true);
        }

        // 현재 레시피 저장
        public void SaveCurrentRecipe()
        {
            lock (_recipeLock)
            {
                if (CurrentRecipe == null) return;
                SafeSaveRecipeNoThrow(CurrentRecipe);
            }
        }

        // 현재 레시피를 새 이름으로 복제 & 전환 (Save As)
        public MeasurementRecipe SaveCurrentAs(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("새 이름이 비어있습니다.", nameof(newName));
            newName = SanitizeRecipeName(newName);

            lock (_recipeLock)
            {
                var src = CurrentRecipe ?? new MeasurementRecipe(newName);
                var clone = (MeasurementRecipe)src.Clone();
                clone.Name = newName;

                SafeSaveRecipeNoThrow(clone);

                CurrentRecipe = clone;
                PersistCurrentRecipeName(newName);
                CurrentRecipeChanged?.Invoke(null, new MeasurementRecipeChangedEventArgs(clone));
                return clone;
            }
        }

        // 현재 인스턴스를 직접 교체 (UI에서 새로 만든 경우)
        public void SetCurrentRecipe(MeasurementRecipe recipe, bool save = true)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            lock (_recipeLock)
            {
                CurrentRecipe = recipe;
                if (save) SafeSaveRecipeNoThrow(recipe);
                PersistCurrentRecipeName(recipe.Name);
                CurrentRecipeChanged?.Invoke(null, new MeasurementRecipeChangedEventArgs(recipe));
            }
        }

        // 내부 공용
        private MeasurementRecipe LoadRecipeInternal(string name, bool raiseEvent)
        {
            name = SanitizeRecipeName(name);
            lock (_recipeLock)
            {
                var r = RecipeManager.LoadOrCreate<MeasurementRecipe>(name);
                CurrentRecipe = r;

                // [CHG] Open 시점에 TeachingRecipeName을 항상 레시피명 기반으로 동기화
                EnsureUnitRecipeNamesBoundToCurrentRecipe();

                // UnitRecipe 로드/마이그레이션
                LoadUnitRecipesForCurrentMeasurementRecipeNoThrow();

                // [ADD] Open 시점에 TeachingRecipeName 변경값을 MeasurementRecipe 파일에 즉시 반영
                try { RecipeManager.Save(CurrentRecipe); } catch { }

                PersistCurrentRecipeName(name);

                if (raiseEvent)
                    CurrentRecipeChanged?.Invoke(null, new MeasurementRecipeChangedEventArgs(r));

                Equipment.Instance.ICurrentRecipe = name;
                return r;
            }
        }

        private void SafeSaveRecipeNoThrow(MeasurementRecipe r)
        {
            try 
            {
                // [ADD] “티칭포지션은 레시피 따라감” 핵심: UnitRecipe 먼저 저장
                EnsureUnitRecipesLoadedForSaveNoThrow();
                SaveUnitRecipesForCurrentMeasurementRecipeNoThrow();

                RecipeManager.Save(r); 
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private static void PersistCurrentRecipeName(string name)
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq.EquipmentConfig == null)
                    eq.EquipmentConfig = EquipmentConfig.LoadOrCreate();

                eq.EquipmentConfig.CurrentRecipeName = name;
                eq.EquipmentConfig.Save(); // 동기 저장 (파일 매우 작음)
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private static string SanitizeRecipeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return _currentRecipeNameFallback;
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name.Trim();
        }

        // ===== [ADD] UnitRecipe 로드/저장 =====

        private void LoadUnitRecipesForCurrentMeasurementRecipeNoThrow()
        {
            try
            {
                if (CurrentRecipe == null)
                    return;

                // 1) 기대 이름으로 먼저 로드/생성
                var expectedName = GetOrLoadIndexChipProbeControllerTeachingRecipeName();
                var loaded = RecipeManager.LoadOrCreate<IndexChipProbeControllerRecipe>(expectedName);

                // 2) 파일이 없던 경우 기본 생성/저장까지 보장(축바인딩은 여기서 불필요)
                loaded.LoadAndBindAxes(axisManager: null);

                // 3) 만약 "expected 파일"이 사실상 비어있고, 레거시 파일이 존재하면 레거시를 마이그레이션
                //    (레거시 -> expected로 복사 저장)
                try
                {
                    var legacyName = GetLegacyTeachingNameForMigration();

                    // 레거시 파일이 실제로 존재하는지 확인 (RecipeManager 규약에 맞춰 경로 추정)
                    // BaseRecipe 규약: Recipes/<TypeName>/<Name>.json
                    var root = AppDomain.CurrentDomain.BaseDirectory;
                    var legacyPath = Path.Combine(root, "Recipes", typeof(IndexChipProbeControllerRecipe).Name, legacyName + ".json");

                    bool hasLegacyFile = File.Exists(legacyPath);
                    bool expectedSeemsEmpty = (loaded.TeachingPositions == null || loaded.TeachingPositions.Count == 0);

                    if (hasLegacyFile && expectedSeemsEmpty)
                    {
                        var legacy = RecipeManager.LoadOrCreate<IndexChipProbeControllerRecipe>(legacyName);
                        legacy.LoadAndBindAxes(axisManager: null);

                        // legacy 내용을 expected에 덮어쓰기
                        loaded.TeachingPositions = legacy.TeachingPositions ?? new System.Collections.Generic.List<TeachingPosition>();
                        loaded.ApplyAxisMapping();

                        // 이름을 expected로 강제 후 저장(=마이그레이션 완료)
                        loaded.Name = expectedName;
                        RecipeManager.Save(loaded);
                    }
                }
                catch { }

                // 4) 캐시 주입 (단일화)
                SetIndexChipProbeControllerTeachingRecipe(loaded);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void SaveUnitRecipesForCurrentMeasurementRecipeNoThrow()
        {
            try
            {
                if (CurrentRecipe == null) return;

                // IndexChipProbeController Teaching
                if (IndexChipProbeControllerTeachingRecipe != null)
                {
                    var expectedName = GetOrLoadIndexChipProbeControllerTeachingRecipeName();
                    if (!string.IsNullOrWhiteSpace(expectedName) &&
                        !string.Equals(IndexChipProbeControllerTeachingRecipe.Name, expectedName, StringComparison.OrdinalIgnoreCase))
                    {
                        IndexChipProbeControllerTeachingRecipe.Name = expectedName;
                    }

                    RecipeManager.Save(IndexChipProbeControllerTeachingRecipe);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
        private void EnsureUnitRecipesLoadedForSaveNoThrow()
        {
            try
            {
                if (IndexChipProbeControllerTeachingRecipe == null)
                    LoadUnitRecipesForCurrentMeasurementRecipeNoThrow();
            }
            catch { }
        }

        private static string SanitizeFilePart(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return _currentRecipeNameFallback;

            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return name.Trim();
        }
        private string GetExpectedProbeTeachingName()
        {
            var baseName = SanitizeFilePart(CurrentRecipe?.Name);
            return baseName + "_ProbeTeaching";
        }
        private string GetLegacyTeachingNameForMigration()
        {
            // 과거 규칙: {RecipeName}_IndexChipProbeControllerTeaching
            var baseName = SanitizeFilePart(CurrentRecipe?.Name);
            return baseName + "_IndexChipProbeControllerTeaching";
        }

        // [ADD] MeasurementRecipe의 UnitRecipeName들을 현재 레시피명 기반으로 강제 설정
        private void EnsureUnitRecipeNamesBoundToCurrentRecipe()
        {
            if (CurrentRecipe == null)
                return;

            var expected = GetExpectedProbeTeachingName();

            if (!string.Equals(CurrentRecipe.IndexChipProbeControllerTeachingRecipeName, expected, StringComparison.OrdinalIgnoreCase))
            {
                CurrentRecipe.IndexChipProbeControllerTeachingRecipeName = expected;
            }
        }




        public class MeasurementRecipeChangedEventArgs : EventArgs
        {
            public MeasurementRecipe Recipe { get; }
            public MeasurementRecipeChangedEventArgs(MeasurementRecipe r) => Recipe = r;
        }

        #endregion
    }

}