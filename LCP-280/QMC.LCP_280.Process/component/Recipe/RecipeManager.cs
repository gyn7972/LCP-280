using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.IO;
using QMC.Common; // for BaseRecipe

//코드 
//BaseRecipe.cs → 공통 Recipe 저장/로드/검증 베이스
//MeasurementRecipe.cs → 실제 장비에서 사용할 Recipe 클래스
//MeasurementKey.cs → 검사 항목 정의
//RecipeManager.cs → Recipe를 Load/Save/관리하는 유틸리티

// 사용 예시
//// 1) Recipe 로드/생성
//var recipe = RecipeManager.LoadOrCreate<MeasurementRecipe>("DefaultRecipe");

//// 2) RecipeKeys → WaferData 에 적용
//WaferManager.Instance.SetRecipeKeys("CARRIER01", 0,
//    recipe.Keys.ConvertAll(k => k.Name));

//// 3) Chip 검사 시 Recipe 기반 판정
//var wafer = WaferManager.Instance.GetWafer("CARRIER01", 0);
//var chip = wafer.GetChipByIndex(0);

//foreach (var key in recipe.Keys)
//{
//    var value = chip.GetMeasure(key.Name);
//    if (value.HasValue)
//    {
//        if (!double.IsNaN(key.LowerLimit) && value.Value < key.LowerLimit)
//            chip.SetReject($"{key.Name} under spec");
//        if (!double.IsNaN(key.UpperLimit) && value.Value > key.UpperLimit)
//            chip.SetReject($"{key.Name} over spec");
//    }
//}

namespace QMC.LCP_280.Process.Component
{
    public static class RecipeManager
    {
        // Generic API (kept for callers that compile fine)
        public static T LoadOrCreate<T>(string name)
            where T : QMC.Common.BaseRecipe, new()
        {
            var r = new T { Name = name };
            QMC.Common.BaseRecipe br = r;
            var path = br.GetFilePath();

            if (!File.Exists(path))
            {
                br.Save();
                return r;
            }

            br.Load();
            return r;
        }

        public static void Save<T>(T recipe) where T : QMC.Common.BaseRecipe
        {
            ((QMC.Common.BaseRecipe)recipe).Save();
        }

        public static bool Delete<T>(string name)
            where T : QMC.Common.BaseRecipe, new()
        {
            try
            {
                var r = new T { Name = name };
                QMC.Common.BaseRecipe br = r;
                var path = br.GetFilePath();
                if (File.Exists(path))
                {
                    File.Delete(path);
                    var bak = path + ".bak";
                    if (File.Exists(bak))
                    {
                        try { File.Delete(bak); } catch { }
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Non-generic API (runtime type) to avoid generic constraint issues at call sites
        public static QMC.Common.BaseRecipe LoadOrCreate(Type recipeType, string name)
        {
            if (recipeType == null) throw new ArgumentNullException(nameof(recipeType));
            if (!typeof(QMC.Common.BaseRecipe).IsAssignableFrom(recipeType))
                throw new ArgumentException("recipeType must derive from BaseRecipe", nameof(recipeType));

            var r = (QMC.Common.BaseRecipe)Activator.CreateInstance(recipeType);
            r.Name = name;
            var path = r.GetFilePath();
            if (!File.Exists(path))
            {
                r.Save();
                return r;
            }
            r.Load();
            return r;
        }

        public static void Save(QMC.Common.BaseRecipe recipe)
        {
            recipe?.Save();
        }

        public static bool Delete(Type recipeType, string name)
        {
            if (recipeType == null) throw new ArgumentNullException(nameof(recipeType));
            if (!typeof(QMC.Common.BaseRecipe).IsAssignableFrom(recipeType)) return false;
            try
            {
                var r = (QMC.Common.BaseRecipe)Activator.CreateInstance(recipeType);
                r.Name = name;
                var path = r.GetFilePath();
                if (File.Exists(path))
                {
                    File.Delete(path);
                    var bak = path + ".bak";
                    if (File.Exists(bak))
                    {
                        try { File.Delete(bak); } catch { }
                    }
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
