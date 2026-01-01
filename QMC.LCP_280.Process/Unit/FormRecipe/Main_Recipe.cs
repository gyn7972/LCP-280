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
        private string _clipboardRecipePath; // copy/paste용

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

            // 전역 변경 이벤트 구독 (폼 닫힐 때 해제 권장)
            EquipmentRecipe.CurrentRecipeChanged += Equipment_CurrentRecipeChanged;
            SelectRecipeInList(_current.Name);
        }

        private void Main_Recipe_FormClosed(object sender, FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            var eq = Equipment.Instance;
            EquipmentRecipe.CurrentRecipeChanged -= Equipment_CurrentRecipeChanged;
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

            // Header: Recipe Name
            _pc.Add(new TitleOnlyProperty(r.Name ?? "Recipe"));

            // Group by Category attribute. If none, group by "General"
            //var props = r.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            //              .Where(p => p.CanRead && p.CanWrite)
            //              .ToList();
            var props = r.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                  .Where(p => p.CanRead) // set 없이도 표시는 가능
                  .ToList();

            var groups = new SortedDictionary<string, List<PropertyInfo>>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in props)
            {
                var cat = p.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() as CategoryAttribute;
                var groupName = cat != null && !string.IsNullOrEmpty(cat.Category) ? cat.Category : "General";
                if (!groups.TryGetValue(groupName, out var list))
                {
                    list = new List<PropertyInfo>();
                    groups[groupName] = list;
                }
                list.Add(p);
            }

            foreach (var kv in groups)
            {
                _pc.Add(new TitleOnlyProperty(kv.Key));
                foreach (var p in kv.Value)
                {
                    if (!IsSupportedPropertyType(p.PropertyType)) continue;

                    var dn = p.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
                    string title = dn != null && !string.IsNullOrEmpty(dn.DisplayName) ? dn.DisplayName : p.Name;

                    var val = p.GetValue(r, null);

                    // 편집 가능 여부 판단:
                    //  - ReadOnly 특성
                    //  - setter 없음
                    //  - 로그인 레벨 미충족
                    var ro = p.GetCustomAttributes(typeof(ReadOnlyAttribute), true).FirstOrDefault() as ReadOnlyAttribute;
                    bool readOnlyAttr = ro?.IsReadOnly == true;
                    bool noSetter = !p.CanWrite;
                    bool loginDenied = !IsEditableByLogin(title);

                    bool isReadOnly = readOnlyAttr || noSetter || loginDenied;

                    _pc.Add(title, "", val);

                    if (isReadOnly)
                    {
                        _uiReadOnlyTitles.Add(title);
                        // 저장 대상에서 제외
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

        //private void ApplyPropertiesToRecipe()
        //{
        //    if (_current == null || _pc == null || _propMap == null) return;

        //    try
        //    {
        //        propertyCollectionView.Apply();

        //        foreach (var prop in _pc)
        //        {
        //            if (prop is TitleOnlyProperty) continue;

        //            if (!_propMap.TryGetValue(prop.Title, out var pi))
        //                continue; // 잠금된 항목 혹은 표시 전용은 스킵

        //            object v = prop.Value;
        //            try
        //            {
        //                if (v != null)
        //                {
        //                    var targetType = pi.PropertyType;
        //                    object converted = v;
        //                    if (targetType.IsEnum)
        //                        converted = Enum.Parse(targetType, v.ToString());
        //                    else if (targetType != v.GetType())
        //                        converted = Convert.ChangeType(v, targetType);

        //                    pi.SetValue(_current, converted, null);
        //                }
        //                else
        //                {
        //                    pi.SetValue(_current, null, null);
        //                }
        //            }
        //            catch { /* 변환/설정 실패는 무시 */ }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("속성 적용 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
        //private void ApplyPropertiesToRecipe()
        //{
        //    if (_current == null || _pc == null || _propMap == null) 
        //        return;

        //    try
        //    {
        //        propertyCollectionView.Apply();

        //        foreach (var prop in _pc)
        //        {
        //            var titleOnly = prop as TitleOnlyProperty;
        //            if (titleOnly != null) continue;

        //            PropertyInfo pi;
        //            if (!_propMap.TryGetValue(prop.Title, out pi)) continue;

        //            object v = prop.Value;
        //            try
        //            {
        //                if (v != null)
        //                {
        //                    var targetType = pi.PropertyType;
        //                    object converted = v;
        //                    if (targetType.IsEnum)
        //                    {
        //                        converted = Enum.Parse(targetType, v.ToString());
        //                    }
        //                    else if (targetType != v.GetType())
        //                    {
        //                        converted = Convert.ChangeType(v, targetType);
        //                    }
        //                    pi.SetValue(_current, converted, null);
        //                }
        //                else
        //                {
        //                    pi.SetValue(_current, null, null);
        //                }
        //            }
        //            catch { }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("속성 적용 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        // ===== Buttons =====
        private void btnNew_Click(object sender, EventArgs e)
        {
            var name = PromptInput("새 레시피 이름을 입력하세요:", "New Recipe");
            if (string.IsNullOrWhiteSpace(name)) return;
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

            //_current = new MeasurementRecipe(name);
            //_current.Reset();
            //BuildPropertyFromRecipe(_current);
            //SaveCurrent();
            //RefreshRecipeList();
            //SelectRecipeInList(name);
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
            if (string.IsNullOrWhiteSpace(name)) return;
            var src = Path.Combine(_recipesDir, name + ".json");
            if (!File.Exists(src)) return;
            _clipboardRecipePath = src; // remember source path
            MessageBox.Show($"'{name}' 레시피가 복사되었습니다. Paste로 붙여넣기 하세요.", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_clipboardRecipePath) || !File.Exists(_clipboardRecipePath))
            {
                MessageBox.Show("붙여넣기할 복사본이 없습니다.", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var newName = PromptInput("붙여넣기할 새 레시피 이름:", "Paste Recipe");
            if (string.IsNullOrWhiteSpace(newName)) return;
            newName = SanitizeFileName(newName);
            var dst = Path.Combine(_recipesDir, newName + ".json");
            try
            {
                File.Copy(_clipboardRecipePath, dst, overwrite: false);

                // 전역 전환
                LoadRecipe(newName);

                // 복사된 레시피의 내부 Name을 새 이름으로 동기화 후 저장
                var eq = Equipment.Instance;
                var current = eq.EquipmentRecipe?.CurrentRecipe;
                if (current != null && !string.Equals(current.Name, newName, StringComparison.OrdinalIgnoreCase))
                {
                    current.Name = newName;
                    // Name 변경이 저장 경로에 반영되도록 현재 레시피 재설정 + 저장
                    eq.EquipmentRecipe.SetCurrentRecipe(current, save: true);
                }
                else
                {
                    // Config에 현재 레시피 이름 반영
                    eq.EquipmentRecipe.SaveCurrentRecipe();
                }

                RefreshRecipeList();
                SelectRecipeInList(newName);
            }
            catch (IOException ioex)
            {
                MessageBox.Show("복사 실패: " + ioex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
    }
}
