using System;
using System.IO;
using QMC.LCP_280.Process.Component;
using QMC.Common;

namespace QMC.LCP_280.Process
{
    // 설비 Recipe 전담 로직 분리
    // 설비 Recipe 전담 로직 분리
    public class EquipmentRecipe
    {
        // ===== Global MeasurementRecipe System =====
        #region Global MeasurementRecipe

        public MeasurementRecipe CurrentRecipe { get; set; }
        public string CurrentRecipeName => CurrentRecipe?.Name ?? _currentRecipeNameFallback;
        public static event EventHandler<MeasurementRecipeChangedEventArgs> CurrentRecipeChanged;

        private static readonly object _recipeLock = new object();
        private static string _currentRecipeNameFallback = "Default";

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
                PersistCurrentRecipeName(name);
                if (raiseEvent)
                    CurrentRecipeChanged?.Invoke(null, new MeasurementRecipeChangedEventArgs(r));

                Equipment.Instance.ICurrentRecipe = name;
                return r;

                
            }
        }

        private static void SafeSaveRecipeNoThrow(MeasurementRecipe r)
        {
            try { RecipeManager.Save(r); }
            catch { /* ignore */ }
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
                //OnErrorOccurred("PersistCurrentRecipeName 실패: " + ex.Message);
            }
        }

        private static string SanitizeRecipeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return _currentRecipeNameFallback;
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }

        public class MeasurementRecipeChangedEventArgs : EventArgs
        {
            public MeasurementRecipe Recipe { get; }
            public MeasurementRecipeChangedEventArgs(MeasurementRecipe r) => Recipe = r;
        }

        #endregion
    }

}