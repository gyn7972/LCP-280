using QMC.Common;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections; // added for IEnumerable checks
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

namespace QMC.LCP_280.Process.Unit.FormRecipe
{
    [FormOrder(1)]
    public partial class Main_Recipe : Form
    {
        // Data
        private QMC.Common.BaseRecipe _current; // 명시적 네임스페이스 사용
        private PropertyCollection _pc;
        private Dictionary<string, PropertyInfo> _propMap; // title -> property
        private string _recipesDir;
        private string _recipesFolderDir;
        private string _clipboardRecipePath; // copy/paste용
        private class RecipeClipboard
        {
            public string JsonPath;
            public string RecipeName;
        }

        // [ADD] UI 읽기전용 타이틀 모음 + 로그인 레벨 규칙
        private readonly HashSet<string> _uiReadOnlyTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // 로그인 레벨(예시: 장비 쪽 로그인 매니저가 있으면 매핑해서 사용)
        private enum LoginLevel { Operator = 0, Engineer = 1, Admin = 2 }

        // 타이틀별 최소 편집 레벨(원하면 Category/Name 기반으로 확장)
        private static readonly Dictionary<string, LoginLevel> _minLevelByTitle =
            new Dictionary<string, LoginLevel>(StringComparer.OrdinalIgnoreCase)
            {
                // 예시 매핑: 필요에 따라 항목 추가/수정
                //{ "Wafer Diameter (mm)", LoginLevel.Engineer },
                //{ "Wafer Chip PitchX (mm)", LoginLevel.Engineer },
                //{ "Wafer Chip PitchY (mm)", LoginLevel.Engineer },
                //{ "Binning Spec Sheet File", LoginLevel.Admin },
                //{ "Test Condition Set File", LoginLevel.Engineer },
                //// 경로/방향 설정 등
                //{ "Wafer Rotate", LoginLevel.Operator },
                //{ "Wafer Mirror", LoginLevel.Operator },
                //{ "Wafer PathStartCorner", LoginLevel.Engineer },
                //{ "Wafer Path PrimaryAxis", LoginLevel.Engineer },
                //{ "Wafer Path TraversalMode", LoginLevel.Engineer },
                //{ "Bin Rotate", LoginLevel.Operator },
                //{ "BIn Mirror", LoginLevel.Operator },
                //{ "BIn PathStartCorner", LoginLevel.Engineer },
                //{ "Bin Path PrimaryAxis", LoginLevel.Engineer },
                //{ "Bin Path TraversalMode", LoginLevel.Engineer },
            };
        private LoginLevel GetCurrentLoginLevel()
        {
            try
            {
                // 실제 로그인 시스템과 연결
                // 예: var lv = Equipment.Instance?.LoginManager?.CurrentLevel;
                // 여기서는 없을 때 기본 Operator로 설정
                return LoginLevel.Operator;
            }
            catch { return LoginLevel.Operator; }
        }

        private bool IsEditableByLogin(string title)
        {
            var cur = GetCurrentLoginLevel();
            if (_minLevelByTitle.TryGetValue(title, out var minLv))
                return cur >= minLv;
            // 매핑이 없으면 기본 허용
            return true;
        }

        public Main_Recipe()
        {
            InitializeComponent();

            // hook events
            this.Load -= Main_Recipe_Load;
            this.Load += Main_Recipe_Load;
            
            //string name = Equipment._CurrentRecipeName;
            //LoadRecipe(name);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                var name = recipeListView.SelectedItemName;
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("삭제할 레시피를 선택하세요.");
                    return;
                }
                var dr = MessageBox.Show($"'{name}' 레시피를 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;
                if (RecipeManager.Delete(typeof(MeasurementRecipe), name))
                {
                    RefreshRecipeList();
                }
                else
                {
                    MessageBox.Show("삭제 실패 또는 파일 없음");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("삭제 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Main_Recipe_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;

            _recipesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes", typeof(MeasurementRecipe).Name);
            Directory.CreateDirectory(_recipesDir);
            RefreshRecipeList();

            // 전역 현재 레시피 UI 반영
            var eq = Equipment.Instance;
            _current = eq.EquipmentRecipe.GetRecipe();
            BuildPropertyFromRecipe(_current);

            // [추가] 값 변경 시 UI 갱신을 위한 이벤트 연결
            // 주의: propertyCollectionView에 해당 이벤트가 있는지 확인하세요.
            //this.propertyCollectionView.ValueChanged -= PropertyCollectionView_ValueChanged;
            //this.propertyCollectionView.ValueChanged += PropertyCollectionView_ValueChanged;

            // 전역 변경 이벤트 구독 (폼 닫힐 때 해제 권장)
            EquipmentRecipe.CurrentRecipeChanged -= Equipment_CurrentRecipeChanged;
            EquipmentRecipe.CurrentRecipeChanged += Equipment_CurrentRecipeChanged;
            SelectRecipeInList(_current.Name);


        }

        private void Main_Recipe_FormClosed(object sender, FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            var eq = Equipment.Instance;
            EquipmentRecipe.CurrentRecipeChanged -= Equipment_CurrentRecipeChanged;
        }

        private void PropertyCollectionView_ValueChanged(object sender, EventArgs e)
        {
            // 이벤트 인자(e)나 sender를 통해 변경된 항목의 이름을 알 수 있다면 
            // "UseSameAsWafer"일 때만 동작하도록 최적화 가능합니다.
            // 여기서는 안전하게 전체 갱신 방식을 사용합니다.

            if (_current == null) 
                return;

            try
            {
                // 1. 현재 UI의 변경된 값(UseSameAsWafer 등)을 실제 객체(_current)에 반영
                propertyCollectionView.Apply();

                // 2. 변경된 값을 바탕으로 다시 UI 빌드 (여기서 ReadOnly 로직이 다시 수행됨)
                BuildPropertyFromRecipe(_current);
            }
            catch { }
        }

        private void Main_Recipe_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                // 폼이 보여질 때마다 현재 레시피 동기화 (다른 폼에서 변경될 때 UI에 반영되지 않는 부분 수정)
                var eq = Equipment.Instance;
                _current = eq.EquipmentRecipe.GetRecipe();
                BuildPropertyFromRecipe(_current);
            }
        }

        private void Equipment_CurrentRecipeChanged(object sender, EquipmentRecipe.MeasurementRecipeChangedEventArgs e)
        {
            try
            {
                _current = e.Recipe;
                BuildPropertyFromRecipe(_current);
                SelectRecipeInList(_current.Name);
            }
            catch { }
        }

        private void RefreshRecipeList()
        {
            try
            {
                var names = Directory.EnumerateFiles(_recipesDir, "*.json")
                                      .Select(Path.GetFileNameWithoutExtension)
                                      .OrderBy(n => n)
                                      .Cast<object>()
                                      .ToArray();
                recipeListView.SetItems(names);
                if (names.Length > 0)
                {
                    recipeListView.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("레시피 목록 로드 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRecipe(string name)
        {
            try
            {
                var eq = Equipment.Instance;
                _current = eq.EquipmentRecipe.LoadRecipe(name);
                BuildPropertyFromRecipe(_current);
                
                // 레시피 Open 이후 반영 처리 ------
                var currentRecipe = eq.EquipmentRecipe.CurrentRecipe;

                var tester = Equipment.Instance.Tester;
                int retLoadTestCondSet = tester.ConditionSet.LoadFromFile(currentRecipe.TestConditionSetFile);
                //int retLoadBinningSpec = tester.BinningSpecSheet.LoadFromFile(currentRecipe.BinningSpecSheetFile);
                int retLoadBinning = tester.LoadBinningModel(currentRecipe.BinningSpecSheetFile);
                if (retLoadBinning != 0 || retLoadTestCondSet != 0)
                {
                    string confirmMessage = "The recipe was opened, but the file below failed to load. Please check.";
                    if (retLoadTestCondSet != 0)
                        confirmMessage += Environment.NewLine + $"- Test Condition Set: {currentRecipe.TestConditionSetFile}";
                    if (retLoadBinning != 0)
                        confirmMessage += Environment.NewLine + $"- Binning Spec Sheet: {currentRecipe.BinningSpecSheetFile}";

                    Equipment.Instance.ResultWriterManager.CurrentTestConditionSet = tester.ConditionSet;

                    MessageBox.Show(confirmMessage, "Confirm");
                }
                // -------------------------------

            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show("레시피 로드 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuildPropertyFromRecipe(QMC.Common.BaseRecipe r)
        {
            _pc = new PropertyCollection();
            _propMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            _uiReadOnlyTitles.Clear();

            if (r == null) { propertyCollectionView.SetProperties(_pc); return; }

            // [변경 1] Header: Recipe Name (원하면 주석 처리하거나 "General" 그룹 안으로 포함 가능)
            // 여기서는 최상단 타이틀로 유지합니다.
            _pc.Add(new TitleOnlyProperty(r.Name ?? "Recipe"));

            // 1. 모든 속성 가져오기
            var props = r.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .ToList();

            // 2. 그룹화 (정렬 전 임시 저장소)
            var rawGroups = new Dictionary<string, List<PropertyInfo>>(StringComparer.OrdinalIgnoreCase);

            // "General" 그룹이 항상 존재하게 하려면 미리 추가
            if (!rawGroups.ContainsKey("General")) rawGroups["General"] = new List<PropertyInfo>();

            foreach (var p in props)
            {
                var cat = p.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() as CategoryAttribute;
                var groupName = cat != null && !string.IsNullOrEmpty(cat.Category) ? cat.Category : "General";

                if (!rawGroups.TryGetValue(groupName, out var list))
                {
                    list = new List<PropertyInfo>();
                    rawGroups[groupName] = list;
                }
                list.Add(p);
            }

            // [변경 2] 카테고리 정렬 (정의된 순서 -> 그 외 알파벳순)
            var sortedGroupNames = rawGroups.Keys
                .OrderBy(k => GetCategorySortIndex(k))
                .ThenBy(k => k)
                .ToList();

            // [추가] 0. UseSameAsWafer 값 미리 확인 (속성 제어용)
            bool isSameAsWafer = false;
            var propUseSame = r.GetType().GetProperty("UseSameAsWafer");
            if (propUseSame != null)
            {
                var val = propUseSame.GetValue(r);
                if (val is bool b) isSameAsWafer = b;
            }

            // 3. 정렬된 그룹 순회 및 속성 추가
            foreach (var groupName in sortedGroupNames)
            {
                var propList = rawGroups[groupName];
                if (propList.Count == 0) continue;

                // 그룹 헤더 추가
                _pc.Add(new TitleOnlyProperty(groupName));

                // [변경 3] 속성 내부 정렬 (정의된 리스트 순서 -> 그 외 DisplayName/이름 순)
                var sortedProps = propList.OrderBy(p =>
                {
                    var dn = p.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
                    string title = dn != null && !string.IsNullOrEmpty(dn.DisplayName) ? dn.DisplayName : p.Name;

                    int index = _propertyOrder.IndexOf(title);
                    // 리스트에 있으면 해당 인덱스, 없으면 큰 값(뒤로 보냄)
                    return index >= 0 ? index : 9999;
                })
                .ThenBy(p =>
                {
                    // 리스트에 없는 항목끼리는 이름순 정렬
                    var dn = p.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
                    return dn != null && !string.IsNullOrEmpty(dn.DisplayName) ? dn.DisplayName : p.Name;
                });

                foreach (var p in sortedProps)
                {
                    if (!IsSupportedPropertyType(p.PropertyType)) 
                        continue;

                    var dn = p.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
                    string title = dn != null && !string.IsNullOrEmpty(dn.DisplayName) ? dn.DisplayName : p.Name;

                    var val = p.GetValue(r, null);

                    // 편집 가능 여부 판단
                    var ro = p.GetCustomAttributes(typeof(ReadOnlyAttribute), true).FirstOrDefault() as ReadOnlyAttribute;
                    bool readOnlyAttr = ro?.IsReadOnly == true;
                    bool noSetter = !p.CanWrite;
                    bool loginDenied = !IsEditableByLogin(title);

                    // [추가] UseSameAsWafer가 true일 때 특정 항목 잠금
                    bool isLockedByWaferOption = false;
                    if (isSameAsWafer)
                    {
                        // 실제 화면에 표시되는 이름(DisplayName)과 정확히 일치해야 합니다.
                        if (title == "Bin Path StartCorner" ||
                            title == "Bin Path PrimaryAxis" || // 띄어쓰기 주의 (DisplayName 확인 필요)
                            title == "Bin Path TraversalMode" ||
                            title == "Bin Diameter (mm)" )          // 필요시 추가
                            //title == "Bin Pitch X (mm)" ||
                            //title == "Bin Pitch Y (mm)" )
                        {
                            isLockedByWaferOption = true;
                        }
                    }

                    // [변경] isLockedByWaferOption 조건 추가
                    bool isReadOnly = readOnlyAttr || noSetter || loginDenied || isLockedByWaferOption;

                    _pc.Add(title, "", val);

                    if (isReadOnly)
                    {
                        _uiReadOnlyTitles.Add(title);
                    }
                    else
                    {
                        if (!_propMap.ContainsKey(title))
                            _propMap.Add(title, p);
                    }
                }
            }

            propertyCollectionView.GroupName = "Property";
            propertyCollectionView.SetProperties(_pc);

            // [중요] UI 편집 컨트롤 비활성화 적용
            ApplyUiReadOnly(_uiReadOnlyTitles);
        }
        
        private void ApplyUiReadOnly(IEnumerable<string> readOnlyTitles)
        {
            try
            {
                // PropertyCollectionView가 타이틀 단위로 편집여부를 제어하는 API를 제공한다고 가정
                // 없을 경우 아래 확장 메서드를 컨트롤에 추가하세요.
                propertyCollectionView.SetReadOnlyTitles(readOnlyTitles.ToArray());
            }
            catch
            {
                // Fallback: 편집 불가가 하나라도 있으면 Save만 막는 등 보수적 처리
                // 이 영역은 필요 시 비워두세요.
            }
        }

        private void ApplyPropertiesToRecipe()
        {
            if (_current == null || _pc == null || _propMap == null) return;

            try
            {
                propertyCollectionView.Apply();

                foreach (var prop in _pc)
                {
                    if (prop is TitleOnlyProperty) continue;
                    
                    // 읽기 전용/로그인 미허용 항목은 _propMap에 없음 → 자동 스킵
                    if (!_propMap.TryGetValue(prop.Title, out var pi))
                        continue;

                    object v = prop.Value;
                    try
                    {
                        if (v != null)
                        {
                            var targetType = pi.PropertyType;
                            object converted = v;
                            if (targetType.IsEnum)
                                converted = Enum.Parse(targetType, v.ToString());
                            else if (targetType != v.GetType())
                                converted = Convert.ChangeType(v, targetType);

                            pi.SetValue(_current, converted, null);
                        }
                        else
                        {
                            pi.SetValue(_current, null, null);
                        }
                    }
                    catch { /* 변환/설정 실패는 무시 */ }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("속성 적용 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // [선택] 로그인 레벨 변경 시 호출해 UI 즉시 갱신
        public void OnLoginLevelChanged()
        {
            var eq = Equipment.Instance;
            _current = eq.EquipmentRecipe.GetRecipe();
            BuildPropertyFromRecipe(_current);
        }

        private static bool IsSupportedPropertyType(Type type)
        {
            // Unwrap Nullable<T>
            var t = Nullable.GetUnderlyingType(type) ?? type;

            if (t.IsEnum) return true;
            if (t == typeof(string)) return true;
            if (t == typeof(bool)) return true;
            if (t == typeof(int)) return true;
            if (t == typeof(long)) return true;
            if (t == typeof(float)) return true;
            if (t == typeof(double)) return true;

            // Exclude IEnumerable (lists, arrays, dictionaries) except string
            if (typeof(IEnumerable).IsAssignableFrom(t)) return false;

            // All other complex types are not supported by PropertyCollection.Add
            return false;
        }

        // ===== Buttons =====
        private void btnNew_Click(object sender, EventArgs e)
        {
            var name = PromptInput("새 레시피 이름을 입력하세요:", "New Recipe");
            if (string.IsNullOrWhiteSpace(name)) return;

            // 클립보드가 있으면 "복사기반 새로만들기"
            if (!string.IsNullOrEmpty(_clipboardRecipePath) && File.Exists(_clipboardRecipePath))
            {
                try
                {
                    CreateFromClipboard(name);
                    MessageBox.Show("복사본 기반 새 레시피 생성 완료.", "New", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("새로 만들기(복사 기반) 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // 기존 "그냥 New"
            name = SanitizeFileName(name);
            var r = new MeasurementRecipe(name);
            r.Reset();
            _current = r;
            var eq = Equipment.Instance;
            eq.EquipmentRecipe.SetCurrentRecipe(r, save: true);
            BuildPropertyFromRecipe(r);
            RefreshRecipeList();
            SelectRecipeInList(name);
            MessageBox.Show("새 레시피 생성 및 전환 완료.", "New", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            var mb = new MessageBoxYesNo();
            var drConfirm = mb.ShowDialog("Confirmation", "Recipe Open?");
            if (drConfirm != DialogResult.Yes)
            {
                return;
            }

            var name = recipeListView.SelectedItemName;
            if (string.IsNullOrWhiteSpace(name))
            {
                Log.Write("MainRecipe", "btnOpen_Click", "Fail.");
                return;
            }

            LoadRecipe(name);
            var mb1 = new MessageBoxOk();
            mb1.ShowDialog("Info!", $"Recipe Open Success.");
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var name = recipeListView.SelectedItemName;
            if (string.IsNullOrWhiteSpace(name))
                return;

            // 레시피 파일 경로(기존 동작 유지)
            var srcRecipe = Path.Combine(_recipesDir, name + ".json");
            if (!File.Exists(srcRecipe)) 
                return;

            // [추가] 레시피와 동일 이름의 폴더도 함께 복사 대상으로 기억
            // 규칙: /Recipes/{TypeName}/{RecipeName}/

            _recipesFolderDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes");
            var srcDir = Path.Combine(_recipesFolderDir, name);

            // json을 기준으로 Copy/Paste를 동작시키되, 폴더는 존재하면 같이 처리
            _clipboardRecipePath = srcRecipe;

            if (Directory.Exists(srcDir))
            {
                MessageBox.Show($"'{name}' 레시피(폴더 포함)가 복사되었습니다. Paste로 붙여넣기 하세요.", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"'{name}' 레시피가 복사되었습니다. Paste로 붙여넣기 하세요.", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            //var name = recipeListView.SelectedItemName;
            //if (string.IsNullOrWhiteSpace(name)) 
            //    return;

            //var src = Path.Combine(_recipesDir, name + ".json");
            //if (!File.Exists(src)) return;
            //_clipboardRecipePath = src; // remember source path
            //MessageBox.Show($"'{name}' 레시피가 복사되었습니다. Paste로 붙여넣기 하세요.", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            // 0) Clipboard 유효성
            if (string.IsNullOrEmpty(_clipboardRecipePath) || !File.Exists(_clipboardRecipePath))
            {
                MessageBox.Show("붙여넣기할 복사본이 없습니다.", "Paste",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 1) 새 이름 입력
            var newName = PromptInput("붙여넣기할 새 레시피 이름:", "Paste Recipe");
            if (string.IsNullOrWhiteSpace(newName)) return;
            newName = SanitizeFileName(newName);

            var oldName = Path.GetFileNameWithoutExtension(_clipboardRecipePath);

            // 2) 대상 json/폴더 경로
            var dstRecipe = Path.Combine(_recipesDir, newName + ".json");
            var srcDir = Path.Combine(_recipesDir, oldName);
            var dstDir = Path.Combine(_recipesDir, newName);

            // 3) 사전 충돌 체크(가독성/사용자 메시지 개선)
            if (File.Exists(dstRecipe))
            {
                MessageBox.Show($"동일한 이름의 레시피가 이미 존재합니다: {newName}", "Paste",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 폴더는 존재해도 json만 복사할 수도 있으니, 폴더 충돌은 srcDir 있을 때만 체크
            if (Directory.Exists(srcDir) && Directory.Exists(dstDir))
            {
                MessageBox.Show($"대상 폴더가 이미 존재합니다:\r\n{dstDir}", "Paste",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 4) json 복사
                File.Copy(_clipboardRecipePath, dstRecipe, overwrite: false);


                _recipesFolderDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes");
                var srcFolderDir = Path.Combine(_recipesFolderDir, oldName);
                var dstFolderDir = Path.Combine(_recipesFolderDir, newName);

                // 5) 폴더 복사 + 치환
                if (Directory.Exists(srcFolderDir))
                {
                    CopyDirectoryRecursive(srcFolderDir, dstFolderDir);

                    // 복사 결과 확인(보수적으로)
                    if (!Directory.Exists(dstFolderDir))
                        throw new IOException($"폴더 복사에 실패했습니다: {dstFolderDir}");

                    // 5-1) 파일/폴더 이름 치환
                    RenamePathsByRecipeName(dstFolderDir, oldName, newName);

                    // 5-2) 폴더 내부 텍스트 내용 치환
                    ReplaceTextContentsByRecipeName(dstFolderDir, oldName, newName);
                }

                // 6) 전역 전환
                LoadRecipe(newName);

                // 7) 복사된 레시피의 내부 Name을 새 이름으로 동기화 후 저장
                var eq = Equipment.Instance;
                var current = eq.EquipmentRecipe?.CurrentRecipe;

                if (current != null && !string.Equals(current.Name, newName, StringComparison.OrdinalIgnoreCase))
                {
                    current.Name = newName;
                    eq.EquipmentRecipe.SetCurrentRecipe(current, save: true);
                }
                else
                {
                    eq.EquipmentRecipe.SaveCurrentRecipe();
                }

                RefreshRecipeList();
                SelectRecipeInList(newName);

                MessageBox.Show($"레시피 붙여넣기 완료: {newName}", "Paste",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (IOException ioex)
            {
                MessageBox.Show("Paste(IO) 실패: " + ioex.Message, "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Paste 중 오류: " + ex.Message, "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== [ADD] Folder copy & rename helpers =====
        private void CreateFromClipboard(string newName)
        {
            newName = SanitizeFileName(newName);

            var oldName = Path.GetFileNameWithoutExtension(_clipboardRecipePath);

            var dstRecipe = Path.Combine(_recipesDir, newName + ".json");
            var srcDir = Path.Combine(_recipesDir, oldName);
            var dstDir = Path.Combine(_recipesDir, newName);

            File.Copy(_clipboardRecipePath, dstRecipe, overwrite: false);

            if (Directory.Exists(srcDir))
            {
                if (Directory.Exists(dstDir))
                    throw new IOException($"대상 폴더가 이미 존재합니다: {dstDir}");

                CopyDirectoryRecursive(srcDir, dstDir);
                RenamePathsByRecipeName(dstDir, oldName, newName);
                ReplaceTextContentsByRecipeName(dstDir, oldName, newName);
            }

            LoadRecipe(newName);

            var eq = Equipment.Instance;
            var current = eq.EquipmentRecipe?.CurrentRecipe;
            if (current != null && !string.Equals(current.Name, newName, StringComparison.OrdinalIgnoreCase))
            {
                current.Name = newName;
                eq.EquipmentRecipe.SetCurrentRecipe(current, save: true);
            }
            else
            {
                eq.EquipmentRecipe.SaveCurrentRecipe();
            }

            RefreshRecipeList();
            SelectRecipeInList(newName);
        }

        private static void CopyDirectoryRecursive(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var dst = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, dst, overwrite: false);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dst = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectoryRecursive(dir, dst);
            }
        }

        private static void RenamePathsByRecipeName(string rootDir, string oldName, string newName)
        {
            // 깊은 경로부터(자식 먼저) rename 해야 충돌이 덜함
            var allDirs = Directory.GetDirectories(rootDir, "*", SearchOption.AllDirectories)
                                   .OrderByDescending(p => p.Length)
                                   .ToArray();
            var allFiles = Directory.GetFiles(rootDir, "*", SearchOption.AllDirectories)
                                    .OrderByDescending(p => p.Length)
                                    .ToArray();

            try
            {
                foreach (var file in allFiles)
                {
                    var dir = Path.GetDirectoryName(file);
                    var fn = Path.GetFileName(file);

                    if (fn.IndexOf(oldName, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    var newFn = ReplaceIgnoreCase(fn, oldName, newName);
                    var newPath = Path.Combine(dir, newFn);

                    if (!string.Equals(file, newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (File.Exists(newPath))
                            throw new IOException($"파일 이름 변경 충돌: {newPath}");
                        File.Move(file, newPath);
                    }
                }

                foreach (var dirPath in allDirs)
                {
                    var parent = Path.GetDirectoryName(dirPath);
                    var dn = Path.GetFileName(dirPath);

                    if (dn.IndexOf(oldName, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    var newDn = ReplaceIgnoreCase(dn, oldName, newName);
                    var newPath = Path.Combine(parent, newDn);

                    if (!string.Equals(dirPath, newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Directory.Exists(newPath))
                            throw new IOException($"폴더 이름 변경 충돌: {newPath}");
                        Directory.Move(dirPath, newPath);
                    }
                }
            }
            catch( Exception ex)
            {
                Log.Write(ex);
            }
        }

        private static void ReplaceTextContentsByRecipeName(string rootDir, string oldName, string newName)
        {
            var textExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".json", ".txt", ".ini", ".cfg", ".xml", ".csv", ".yaml", ".yml", ".png",
                // TODO: PatternMatching 실제 확장자 있으면 여기에 추가
                // ".pat", ".pm", ".dat"
            };

            var files = Directory.GetFiles(rootDir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);
                if (!textExts.Contains(ext)) continue;

                string text = null;

                // 인코딩 이슈 대비: UTF-8 먼저, 실패하면 Default(ANSI)로 시도
                try
                {
                    text = File.ReadAllText(file, System.Text.Encoding.UTF8);
                }
                catch
                {
                    try
                    {
                        text = File.ReadAllText(file, System.Text.Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (text.IndexOf(oldName, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                var replaced = ReplaceIgnoreCase(text, oldName, newName);
                if (!string.Equals(text, replaced, StringComparison.Ordinal))
                {
                    // 저장은 원본 인코딩 보존이 더 안전하지만,
                    // 우선 Default로 저장(설비 cfg/txt가 ANSI인 경우가 많음)
                    File.WriteAllText(file, replaced, System.Text.Encoding.Default);
                }
            }
        }

        private static string ReplaceIgnoreCase(string input, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(oldValue))
                return input;

            return System.Text.RegularExpressions.Regex.Replace(
                input,
                System.Text.RegularExpressions.Regex.Escape(oldValue),
                newValue.Replace("$", "$$"),
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveCurrent();
        }

        private void SaveCurrent()
        {
            var mb = new MessageBoxOk();
            try
            {
                if (_current == null)
                {
                    mb.ShowDialog("Information!", $"Recipe None.");
                    return;
                }
                ApplyPropertiesToRecipe();
                var eq = Equipment.Instance;
                eq.EquipmentRecipe.SaveCurrentRecipe();
                RefreshRecipeList();
                SelectRecipeInList(_current.Name);

                mb.ShowDialog("Information!", $"Save Sucess.");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                mb.ShowDialog("Error!", $"Save Fail.");
            }
        }

        private static string PromptInput(string text, string caption)
        {
            using (var f = new Form())
            {
                f.Text = caption;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ClientSize = new Size(360, 120);
                var label = new Label { Text = text, AutoSize = true, Location = new Point(12, 12) };
                var tb = new TextBox { Left = 12, Top = 40, Width = 330 };
                var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 190, Width = 70, Top = 76 };
                var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 272, Width = 70, Top = 76 };
                f.Controls.AddRange(new Control[] { label, tb, ok, cancel });
                f.AcceptButton = ok; f.CancelButton = cancel;
                return f.ShowDialog() == DialogResult.OK ? tb.Text : null;
            }
        }

        private void SelectRecipeInList(string name)
        {
            try
            {
                var items = recipeListView.GetItems();
                for (int i = 0; i < items.Length; i++)
                {
                    if (string.Equals(items[i], name, StringComparison.OrdinalIgnoreCase))
                    {
                        recipeListView.SelectedIndex = i;
                        break;
                    }
                }
            }
            catch { }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }

        // Host size handling (optional reflection target)
        public void SetPanelSize(int width, int height)
        {
            try
            {
                this.SuspendLayout();
                this.Size = new Size(width, height);
                this.ClientSize = new Size(width, height);
                this.Invalidate();
                this.Update();
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        #region Ordering Definitions
        // 1. 카테고리 표시 순서 정의
        private readonly Dictionary<string, int> _categoryOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "General", 0 },      // 최상단
            { "Common", 1 },       // 그 다음
            { "TAlgin", 2 },
            { "Wafer Map", 3 },    // 예시: 실제 사용되는 카테고리 이름
            { "Bin Array", 4 },
            { "Measurement", 5 },
                   
            // 필요 시 추가...
        };

        // 2. 속성 표시 순서 정의 (DisplayName 또는 PropertyName)
        private readonly List<string> _propertyOrder = new List<string>
        {
            // General
            "Name",
    
            // Common / Simulation
            "Simulation",
            "DryRun",

            // Wafer Map
            "Wafer Diameter (mm)",
            "Wafer Chip PitchX (mm)",
            "Wafer Chip PitchY (mm)",
    
            // ... 원하는 순서대로 계속 추가
        };

        // 3. 카테고리 정렬 도우미 메서드
        private int GetCategorySortIndex(string category)
        {
            return _categoryOrder.ContainsKey(category) ? _categoryOrder[category] : 999; // 정의되지 않은 항목은 맨 뒤로
        }
        #endregion
    }
}
