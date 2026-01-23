using QMC.Common;
using QMC.Common.Component;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
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

        // ===== [CHG] UnitRecipe 공용 캐시 =====
        private readonly Dictionary<string, QMC.Common.BaseRecipe> _unitRecipeCache =
            new Dictionary<string, QMC.Common.BaseRecipe>(StringComparer.OrdinalIgnoreCase);

        // UnitKey (파일명 suffix로 사용)
        public const string UnitKey_IndexChipProbeControllerTeaching = "ProbeTeaching";
        public const string UnitKey_IndexLoadAlignerTeaching = "LoadAlignTeaching";
        public const string UnitKey_InputDieTransferTeaching = "InputDieTransferTeaching";
        public const string UnitKey_OutputDieTransferTeaching = "OutputDieTransferTeaching";

        // 기존 외부 접근용 프로퍼티 유지
        public IndexChipProbeControllerRecipe IndexChipProbeControllerTeachingRecipe { get; set; }

        // ===== [ADD] LoadAligner Teaching Recipe 캐시 =====
        public IndexLoadAlignerRecipe IndexLoadAlignerTeachingRecipe { get; set; }

        public InputDieTransferRecipe InputDieTransferTeachingRecipe { get; set; }

        public OutputDieTransferRecipe OutputDieTransferTeachingRecipe { get; set; }

        // ===== [ADD] UnitRecipe 공용 함수 =====

        private string GetExpectedUnitRecipeName(string unitKey)
        {
            var baseName = SanitizeFilePart(CurrentRecipe?.Name);
            var key = SanitizeFilePart(unitKey);
            return baseName + "_" + key;
        }

        private TUnit GetOrLoadUnitRecipe<TUnit>(
            string unitKey,
            Func<string> legacyNameProvider,
            Func<TUnit, bool> expectedSeemsEmpty,
            Action<TUnit> afterLoad,
            Action<TUnit, TUnit> migrateAction)
            where TUnit : QMC.Common.BaseRecipe, new()
        {
            if (string.IsNullOrWhiteSpace(unitKey))
                throw new ArgumentException("unitKey가 비어있습니다.", nameof(unitKey));

            if (CurrentRecipe == null)
                return RecipeManager.LoadOrCreate<TUnit>(_currentRecipeNameFallback + "_" + unitKey);

            var expectedName = GetExpectedUnitRecipeName(unitKey);

            QMC.Common.BaseRecipe cached;
            if (_unitRecipeCache.TryGetValue(unitKey, out cached))
            {
                var typed = cached as TUnit;
                if (typed != null)
                {
                    if (!string.Equals(typed.Name, expectedName, StringComparison.OrdinalIgnoreCase))
                        typed.Name = expectedName;
                    return typed;
                }
            }

            // 1) expected로 먼저 로드/생성
            var loaded = RecipeManager.LoadOrCreate<TUnit>(expectedName);

            // 2) Unit별 후처리
            try { afterLoad?.Invoke(loaded); } catch { }

            // 3) 레거시 마이그레이션
            try
            {
                var legacyName = legacyNameProvider != null ? legacyNameProvider() : null;
                if (!string.IsNullOrWhiteSpace(legacyName))
                {
                    var root = AppDomain.CurrentDomain.BaseDirectory;
                    var legacyPath = Path.Combine(root, "Recipes", typeof(TUnit).Name, legacyName + ".json");

                    bool hasLegacyFile = File.Exists(legacyPath);
                    bool isEmpty = expectedSeemsEmpty != null ? expectedSeemsEmpty(loaded) : false;

                    if (hasLegacyFile && isEmpty)
                    {
                        var legacy = RecipeManager.LoadOrCreate<TUnit>(legacyName);
                        try { afterLoad?.Invoke(legacy); } catch { }

                        if (migrateAction != null)
                            migrateAction(loaded, legacy);

                        loaded.Name = expectedName;
                        RecipeManager.Save(loaded);
                    }
                }
            }
            catch { }

            _unitRecipeCache[unitKey] = loaded;
            return loaded;
        }

        private void SaveUnitRecipe<TUnit>(string unitKey, TUnit recipe)
            where TUnit : QMC.Common.BaseRecipe
        {
            if (recipe == null) return;

            var expectedName = GetExpectedUnitRecipeName(unitKey);
            if (!string.Equals(recipe.Name, expectedName, StringComparison.OrdinalIgnoreCase))
                recipe.Name = expectedName;

            try { RecipeManager.Save(recipe); } catch (Exception ex) { Log.Write(ex); }

            _unitRecipeCache[unitKey] = recipe;
        }

        /// <summary>
        /// (공용) MeasurementRecipe 기준 Teaching UnitRecipeName 정규화 후 기대 이름 반환
        /// - 외부(Config/UI 등)에서 공통으로 사용 가능
        /// </summary>
        public string GetOrLoadUnitTeachingRecipeName(string unitKey)
        {
            if (string.IsNullOrWhiteSpace(unitKey))
                throw new ArgumentException("unitKey가 비어있습니다.", nameof(unitKey));

            if (CurrentRecipe == null)
                return _currentRecipeNameFallback + "_" + SanitizeFilePart(unitKey);

            return GetExpectedUnitRecipeName(unitKey);
        }

        /// <summary>
        /// (공용) 외부에서 로드한 UnitRecipe 인스턴스를 캐시에 주입(단일화)하고,
        /// 이름을 기대 규칙으로 동기화한다.
        /// </summary>
        public void SetUnitTeachingRecipe(string unitKey, QMC.Common.BaseRecipe recipe, bool save = false)
        {
            if (string.IsNullOrWhiteSpace(unitKey))
                throw new ArgumentException("unitKey가 비어있습니다.", nameof(unitKey));
            if (recipe == null) return;

            var expectedName = GetOrLoadUnitTeachingRecipeName(unitKey);
            if (!string.Equals(recipe.Name, expectedName, StringComparison.OrdinalIgnoreCase))
                recipe.Name = expectedName;

            _unitRecipeCache[unitKey] = recipe;

            // 알려진 UnitKey는 Strongly-typed 프로퍼티도 동기화
            if (string.Equals(unitKey, UnitKey_IndexChipProbeControllerTeaching, StringComparison.OrdinalIgnoreCase))
                IndexChipProbeControllerTeachingRecipe = recipe as IndexChipProbeControllerRecipe;
            else if (string.Equals(unitKey, UnitKey_IndexLoadAlignerTeaching, StringComparison.OrdinalIgnoreCase))
                IndexLoadAlignerTeachingRecipe = recipe as IndexLoadAlignerRecipe;
            else if (string.Equals(unitKey, UnitKey_InputDieTransferTeaching, StringComparison.OrdinalIgnoreCase))
                InputDieTransferTeachingRecipe = recipe as InputDieTransferRecipe;
            else if (string.Equals(unitKey, UnitKey_OutputDieTransferTeaching, StringComparison.OrdinalIgnoreCase))
                OutputDieTransferTeachingRecipe = recipe as OutputDieTransferRecipe;

            if (save)
            {
                try { RecipeManager.Save(recipe); } catch (Exception ex) { Log.Write(ex); }
            }
        }

        // ===== [ADD] 기존 호출부 호환용 wrapper (깨짐 방지) =====
        internal string GetOrLoadIndexChipProbeControllerTeachingRecipeName()
        {
            return GetOrLoadUnitTeachingRecipeName(UnitKey_IndexChipProbeControllerTeaching);
        }

        internal void SetIndexChipProbeControllerTeachingRecipe(IndexChipProbeControllerRecipe recipe)
        {
            if (recipe == null)
                return;

            IndexChipProbeControllerTeachingRecipe = recipe;
            SetUnitTeachingRecipe(UnitKey_IndexChipProbeControllerTeaching, recipe, save: false);
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
                // UnitRecipe 먼저 저장
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
                eq.EquipmentConfig.Save();
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

        // ===== [CHG] UnitRecipe 로드/저장 (IndexChipProbeController/IndexLoadAligner 공용 처리) =====

        private void LoadUnitRecipesForCurrentMeasurementRecipeNoThrow()
        {
            try
            {
                if (CurrentRecipe == null)
                    return;

                // 1) ProbeController Teaching
                var probe = GetOrLoadUnitRecipe<IndexChipProbeControllerRecipe>(
                    unitKey: UnitKey_IndexChipProbeControllerTeaching,
                    legacyNameProvider: GetLegacyTeachingNameForMigration,
                    expectedSeemsEmpty: r => (r.TeachingPositions == null || r.TeachingPositions.Count == 0),
                    afterLoad: r => r.LoadAndBindAxes(axisManager: null),
                    migrateAction: (expected, legacy) =>
                    {
                        expected.TeachingPositions = legacy.TeachingPositions ?? new List<TeachingPosition>();
                        expected.ApplyAxisMapping();
                    });

                IndexChipProbeControllerTeachingRecipe = probe;
                _unitRecipeCache[UnitKey_IndexChipProbeControllerTeaching] = probe;

                // 2) LoadAligner Teaching
                var loadAlign = GetOrLoadUnitRecipe<IndexLoadAlignerRecipe>(
                    unitKey: UnitKey_IndexLoadAlignerTeaching,
                    legacyNameProvider: null,
                    expectedSeemsEmpty: r => (r.TeachingPositions == null || r.TeachingPositions.Count == 0),
                    afterLoad: r => r.LoadAndBindAxes(axisManager: null),
                    migrateAction: null);

                IndexLoadAlignerTeachingRecipe = loadAlign;
                _unitRecipeCache[UnitKey_IndexLoadAlignerTeaching] = loadAlign;

                var InputArm = GetOrLoadUnitRecipe<InputDieTransferRecipe>(
                    unitKey: UnitKey_InputDieTransferTeaching,
                    legacyNameProvider: null,
                    expectedSeemsEmpty: r => (r.TeachingPositions == null || r.TeachingPositions.Count == 0),
                    afterLoad: r => r.LoadAndBindAxes(axisManager: null),
                    migrateAction: null);

                InputDieTransferTeachingRecipe = InputArm;
                _unitRecipeCache[UnitKey_InputDieTransferTeaching] = InputArm;

                var OutputArm = GetOrLoadUnitRecipe<OutputDieTransferRecipe>(
                    unitKey: UnitKey_OutputDieTransferTeaching,
                    legacyNameProvider: null,
                    expectedSeemsEmpty: r => (r.TeachingPositions == null || r.TeachingPositions.Count == 0),
                    afterLoad: r => r.LoadAndBindAxes(axisManager: null),
                    migrateAction: null);

                OutputDieTransferTeachingRecipe = OutputArm;
                _unitRecipeCache[UnitKey_OutputDieTransferTeaching] = OutputArm;
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

                if (IndexChipProbeControllerTeachingRecipe != null)
                    SaveUnitRecipe(UnitKey_IndexChipProbeControllerTeaching, IndexChipProbeControllerTeachingRecipe);

                if (IndexLoadAlignerTeachingRecipe != null)
                    SaveUnitRecipe(UnitKey_IndexLoadAlignerTeaching, IndexLoadAlignerTeachingRecipe);

                if (InputDieTransferTeachingRecipe != null)
                    SaveUnitRecipe(UnitKey_InputDieTransferTeaching, InputDieTransferTeachingRecipe);

                if(OutputDieTransferTeachingRecipe != null)
                    SaveUnitRecipe(UnitKey_OutputDieTransferTeaching, OutputDieTransferTeachingRecipe);
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
                if (IndexChipProbeControllerTeachingRecipe == null || IndexLoadAlignerTeachingRecipe == null)
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
            var baseName = SanitizeFilePart(CurrentRecipe?.Name);
            return baseName + "_IndexChipProbeControllerTeaching";
        }


        public class MeasurementRecipeChangedEventArgs : EventArgs
        {
            public MeasurementRecipe Recipe { get; }
            public MeasurementRecipeChangedEventArgs(MeasurementRecipe r) => Recipe = r;
        }

        #endregion
    }
}