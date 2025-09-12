using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using QMC.Common.Sequence;
using QMC.LCP_280.Process.Unit;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Sequences
{
    public partial class ManualSequenceControl : UserControl
    {
        private SequenceBase _sequence;
        private readonly Dictionary<string, SequenceBase> _sequences = new Dictionary<string, SequenceBase>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Func<string[]>> _stepNameProviders = new Dictionary<string, Func<string[]>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Func<string, bool>> _startSingleHandlers = new Dictionary<string, Func<string, bool>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Func<int, string>> _indexNameResolvers = new Dictionary<string, Func<int, string>>(StringComparer.OrdinalIgnoreCase);

        private Func<string[]> _getStepNames;
        private Func<string, bool> _startSingle;
        private Func<int, string> _stepIndexToName;

        private bool _initialized;
        private bool _runtimeInitTried;
        private bool _stepListInitialized;

        private const string StepPrefix = "[STEP]";

        // ===== 추가: 순차 실행 상태 =====
        private bool _enforceSequential = true;      // 항상 순차 제어
        private int _manualStepIndex = -1;           // 현재 실행된(또는 실행 예정) Step 인덱스
        private string[] _cachedStepNames = new string[0];
        private bool _firstConfirmShown = false;     // 최초 1회만 확인창
        private bool _loopSequential = true;   // 마지막 Step 후 처음부터 다시 실행 허용 (수동 순차)

        #region Design Mode Helper
        private bool IsDesign
        {
            get
            {
                if ( LicenseManager.UsageMode==LicenseUsageMode.Designtime ) return true;
                if ( DesignMode ) return true;
                var svc = GetService(typeof(System.ComponentModel.Design.IDesignerHost));
                return svc != null;
            }
        }
        #endregion

        public ManualSequenceControl()
        {
            InitializeComponent();
            if ( IsDesign ) TryPopulateDesignTimeSample();
            WireUiEvents();
        }

        private void WireUiEvents()
        {
            try { _cboSequence.SelectedIndexChanged += ( s, e ) => OnSequenceSelectionChanged(); } catch { }
        }

        #region Multi Sequence API
        public void ClearSequences()
        {
            DetachSequenceEvents();
            foreach ( var kv in _sequences ) { try { kv.Value.Dispose(); } catch { } }
            _sequences.Clear();
            _stepNameProviders.Clear();
            _startSingleHandlers.Clear();
            _indexNameResolvers.Clear();
            _sequence = null;
            _cboSequence?.Items.Clear();
            _lstStep?.Items.Clear();
            ResetSequentialState();
            UpdateButtonStates();
        }

        public void RegisterSequence( string key, SequenceBase sequence,
            Func<string[]> stepNameProvider = null,
            Func<string, bool> startSingleHandler = null,
            Func<int, string> stepIndexNameResolver = null,
            bool autoSelect = false )
        {
            if ( string.IsNullOrWhiteSpace( key ) || sequence == null ) return;
            if ( _sequences.ContainsKey( key ) ) return;
            _sequences[key] = sequence;
            if ( stepNameProvider != null ) _stepNameProviders[key] = stepNameProvider;
            if ( startSingleHandler != null ) _startSingleHandlers[key] = startSingleHandler;
            if ( stepIndexNameResolver != null ) _indexNameResolvers[key] = stepIndexNameResolver;
            _cboSequence.Items.Add( key );
            if ( autoSelect || _cboSequence.SelectedIndex < 0 )
                _cboSequence.SelectedItem = key;
        }

        public void UnregisterSequence( string key )
        {
            if ( string.IsNullOrWhiteSpace( key ) ) return;
            if ( _sequences.TryGetValue( key, out var seq ) )
            {
                if ( ReferenceEquals( seq, _sequence ) )
                {
                    DetachSequenceEvents();
                    _sequence = null;
                }
                try { seq.Dispose(); } catch { }
            }
            _sequences.Remove( key );
            _stepNameProviders.Remove( key );
            _startSingleHandlers.Remove( key );
            _indexNameResolvers.Remove( key );
            _cboSequence.Items.Remove( key );
            if ( _cboSequence.SelectedIndex < 0 && _cboSequence.Items.Count > 0 )
                _cboSequence.SelectedIndex = 0;
            else
                RebindActiveFromCombo();
        }

        private void OnSequenceSelectionChanged()
        {
            RebindActiveFromCombo();
        }

        private void RebindActiveFromCombo()
        {
            string sel = _cboSequence.SelectedItem as string;
            if ( string.IsNullOrEmpty( sel ) || !_sequences.TryGetValue( sel, out var seq ) )
            {
                DetachSequenceEvents();
                _sequence = null;
                _getStepNames = null; _startSingle = null; _stepIndexToName = null;
                _lstStep.Items.Clear();
                ResetSequentialState();
                UpdateButtonStates();
                return;
            }

            DetachSequenceEvents();
            _sequence = seq;
            _getStepNames = _stepNameProviders.TryGetValue( sel, out var p ) ? p : null;
            _startSingle = _startSingleHandlers.TryGetValue( sel, out var s ) ? s : null;
            _stepIndexToName = _indexNameResolvers.TryGetValue( sel, out var r ) ? r : null;
            WireSequenceEvents();

            _stepListInitialized = false;
            ResetSequentialState();
            PopulateStepList();
            UpdateButtonStates();
        }
        #endregion

        #region Legacy Single Sequence API
        public void SetSequence( SequenceBase sequence,
            Func<string[]> stepNameProvider = null,
            Func<string, bool> startSingleHandler = null,
            Func<int, string> stepIndexNameResolver = null )
        {
            ClearSequences();
            RegisterSequence( "(default)", sequence, stepNameProvider, startSingleHandler, stepIndexNameResolver, autoSelect: true );
        }
        #endregion

        #region Populate Step List
        private void PopulateStepList()
        {
            if ( IsDesign ) return;
            if ( _stepListInitialized ) return;
            try
            {
                _lstStep.Items.Clear();
                var names = GetStepNamesInternal();
                _cachedStepNames = names ?? new string[0];
                if ( names != null )
                {
                    foreach ( var s in names )
                        _lstStep.Items.Add( $"{StepPrefix} {s}" );
                }
                _lstStep.Items.Add( "-" );
                _lstStep.Items.Add( "수동 실행: Manual ▶ (순차), Back: 이전 Step" );
                _stepListInitialized = true;

                // 순차 강제 시 사용자 직접 선택 금지
                _lstStep.Enabled = !_enforceSequential || _startSingle == null;
            }
            catch { }
        }

        private string[] GetStepNamesInternal()
        {
            if ( _getStepNames != null )
            {
                try { return _getStepNames(); } catch { }
            }
            if ( _sequence != null )
            {
                var t = _sequence.GetType();
                var mi = t.GetMethod( "GetStepNames", BindingFlags.Public | BindingFlags.Static );
                if ( mi != null && mi.ReturnType == typeof( string[] ) )
                {
                    try { return (string[])mi.Invoke( null, null ); } catch { }
                }
                var stepEnum = t.GetNestedType( "Step", BindingFlags.Public | BindingFlags.NonPublic );
                if ( stepEnum != null && stepEnum.IsEnum )
                {
                    try { return Enum.GetNames( stepEnum ); } catch { }
                }
            }
            return new string[0];
        }
        #endregion

        #region Single Step Helpers
        private bool TryGetSelectedSingleStep( out string stepName )
        {
            stepName = null;
            var sel = _lstStep.SelectedItem as string;
            if ( string.IsNullOrWhiteSpace( sel ) ) return false;
            if ( !sel.StartsWith( StepPrefix ) ) return false;
            stepName = sel.Substring( StepPrefix.Length ).Trim();
            return !string.IsNullOrEmpty( stepName );
        }
        #endregion

        #region Lifecycle / Auto Init
        protected override void OnHandleCreated( EventArgs e )
        {
            base.OnHandleCreated( e );
            if ( IsDesign ) return;
            if ( _runtimeInitTried ) return;
            _runtimeInitTried = true;
            if ( _sequences.Count == 0 )
                TryAutoInitInputStageSequence();
            PopulateStepList();
            UpdateButtonStates();
        }

        private void TryAutoInitInputStageSequence()
        {
            try
            {
                if ( !Equipment.Instance.Units.TryGetValue( "InputStage", out var baseUnit ) ) return;
                var unit = baseUnit as InputStage; if ( unit == null ) return;
                var asm = typeof( InputStage ).Assembly;
                var seqType = asm.GetType( "QMC.LCP_280.Process.Component.SeqInputStage" )
                              ?? asm.GetType( "QMC.LCP_280.Process.Component.Seq_InputStage" );
                if ( seqType == null ) return;
                var create = seqType.GetMethod( "CreateFromUnit", BindingFlags.Public | BindingFlags.Static );
                object seqObj = null;
                if ( create != null )
                    seqObj = create.Invoke( null, new object[] { unit } );
                else
                    seqObj = Activator.CreateInstance( seqType, nonPublic: true );
                if ( !( seqObj is SequenceBase seqBase ) ) return;

                Func<string, bool> startSingle = null;
                var miStartSingle = seqType.GetMethod( "StartSingle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
                if ( miStartSingle != null && miStartSingle.ReturnType == typeof( bool ) && miStartSingle.GetParameters().Length == 1 )
                    startSingle = ( name ) => { try { return (bool)miStartSingle.Invoke( seqBase, new object[] { name } ); } catch { return false; } };
                Func<int, string> indexToName = idx =>
                {
                    try
                    {
                        var stepEnum = seqType.GetNestedType( "Step", BindingFlags.NonPublic | BindingFlags.Public );
                        if ( stepEnum != null && stepEnum.IsEnum && Enum.IsDefined( stepEnum, idx ) )
                            return Enum.GetName( stepEnum, idx );
                    }
                    catch { }
                    return null;
                };
                Func<string[]> getNames = () => GetStepNamesViaReflection( seqType ) ?? new string[0];
                RegisterSequence( "InputStage", seqBase, getNames, startSingle, indexToName, autoSelect: true );
            }
            catch { }
        }

        private static string[] GetStepNamesViaReflection( Type seqType )
        {
            try
            {
                var mi = seqType.GetMethod( "GetStepNames", BindingFlags.Public | BindingFlags.Static );
                if ( mi != null && mi.ReturnType == typeof( string[] ) )
                    return (string[])mi.Invoke( null, null );
                var stepEnum = seqType.GetNestedType( "Step", BindingFlags.Public | BindingFlags.NonPublic );
                if ( stepEnum != null && stepEnum.IsEnum ) return Enum.GetNames( stepEnum );
            }
            catch { }
            return null;
        }
        #endregion

        #region Sequence Events
        private void WireSequenceEvents()
        {
            if ( _sequence == null ) return;
            _sequence.StateChanged += OnSequenceStateChanged;
            _sequence.StepChanged += OnSequenceStepChanged;
            _sequence.ErrorOccurred += OnSequenceError;
            _sequence.Completed += OnSequenceCompleted;
        }
        private void DetachSequenceEvents()
        {
            if ( _sequence == null ) return;
            _sequence.StateChanged -= OnSequenceStateChanged;
            _sequence.StepChanged -= OnSequenceStepChanged;
            _sequence.ErrorOccurred -= OnSequenceError;
            _sequence.Completed -= OnSequenceCompleted;
        }

        private void OnSequenceStateChanged( SequenceBase seq, SequenceState oldS, SequenceState newS ) => SafeUI( UpdateButtonStates );
        private void OnSequenceStepChanged( SequenceBase seq, int step ) => SafeUI( () => HighlightRunningStep( step ) );
        private void OnSequenceError( SequenceBase seq, Exception ex )
        {
            SafeUI(() =>
            {
                // Error 발생 시 수동 순차 실행 인덱스를 처음(-1)으로 리셋하여
                // 다음 Manual ▶ 클릭 시 0번 Step 부터 다시 실행되도록 함.
                if (_enforceSequential && _startSingle != null)
                {
                    _manualStepIndex = -1;
                    try
                    {
                        // 선택 해제 (처음부터 다시 시작 상태 표현)
                        _lstStep.ClearSelected();
                    }
                    catch { }
                }
                UpdateButtonStates();
            });
        }
        private void OnSequenceCompleted( SequenceBase seq )
        {
            SafeUI(() =>
            {
                // 루프 모드일 때 마지막 Step 까지 수동으로 온 상태라면 다음 Manual ▶ 시 0부터
                if (_enforceSequential && _startSingle != null && _loopSequential)
                {
                    if (_cachedStepNames == null || _cachedStepNames.Length == 0)
                        _cachedStepNames = GetStepNamesInternal();

                    if (_manualStepIndex >= 0 && _manualStepIndex >= LastStepIndex)
                    {
                        // 다음 Manual ▶ 클릭 시 첫 Step 재실행하도록 준비
                        _manualStepIndex = -1;
                    }
                }
                UpdateButtonStates();
            });
            //SafeUI( () =>
            //{
            //    UpdateButtonStates();
            //} );
        }
        #endregion

        #region 순차 실행 전용 내부 헬퍼
        private void ResetSequentialState()
        {
            _manualStepIndex = -1;
            _firstConfirmShown = false;
            _cachedStepNames = new string[0];
        }

        private void RunSequentialStep( int targetIndex )
        {
            if ( _startSingle == null ) return;
            if ( _cachedStepNames == null || _cachedStepNames.Length == 0 ) _cachedStepNames = GetStepNamesInternal();
            if ( targetIndex < 0 || targetIndex >= _cachedStepNames.Length ) return;

            _manualStepIndex = targetIndex;

            // ListBox 하이라이트
            for ( int i = 0, realIdx = 0; i < _lstStep.Items.Count; i++ )
            {
                var item = _lstStep.Items[i] as string;
                if ( item != null && item.StartsWith( StepPrefix ) )
                {
                    if ( realIdx == _manualStepIndex )
                    {
                        _lstStep.SelectedIndex = i;
                        break;
                    }
                    realIdx++;
                }
            }

            var stepName = _cachedStepNames[_manualStepIndex];
            try
            {
                var ok = _startSingle(stepName);   // 여기서 StartSingle 호출
                if ( !ok )
                {
                    MessageBox.Show( $"Step 실행 실패: {stepName}", "Manual", MessageBoxButtons.OK, MessageBoxIcon.Warning );
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show( "수동 Step 실행 중 오류\r\n" + ex.Message, "Manual", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
            UpdateButtonStates();
        }

        private int LastStepIndex => _cachedStepNames == null ? -1 : _cachedStepNames.Length - 1;
        #endregion


        // 한 스탭씩 실행 됨.
        #region Button Actions
        private void OnManualClick(object sender, EventArgs e)
        {
            if (IsDesign) return;
            if (_sequence == null) return;

            // 전체 실행을 Single Step 로직으로 대체 (StartSingle 필요)
            if (_startSingle == null)
            {
                // 단일 실행 지원이 없으면 기존처럼 전체 Start 한번만 허용
                if (!_sequence.IsRunning && !_sequence.IsPaused && !_sequence.IsError)
                {
                    _sequence.Start();
                    UpdateButtonStates();
                }
                return;
            }

            // 실행 중이면(해당 Step 동작 완료 대기) 무시
            if (_sequence.IsRunning || _sequence.IsPaused) return;

            // Step 목록 캐시 없으면 갱신
            if (_cachedStepNames == null || _cachedStepNames.Length == 0)
                _cachedStepNames = GetStepNamesInternal();

            if (_cachedStepNames.Length == 0)
            {
                MessageBox.Show("등록된 Step 이 없습니다.", "Manual", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 다음 인덱스 계산
            int nextIndex = _manualStepIndex + 1;
            if (_manualStepIndex < 0) nextIndex = 0;

            //마지막 시컨스 지정.
            if (nextIndex >= _cachedStepNames.Length - 2)
            {
                if (_loopSequential)
                {
                    // 되감기
                    _manualStepIndex = -1;
                    nextIndex = 0;
                }
                else
                {
                    MessageBox.Show("마지막 Step 입니다.", "Manual", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateButtonStates();
                    return;
                }
            }

            RunSequentialStep(nextIndex);
        }

        private void OnBackClick(object sender, EventArgs e)
        {
            if (IsDesign) return;
            if (_sequence == null) return;
            if (_startSingle == null) return;
            if (_sequence.IsRunning || _sequence.IsPaused) return;
            if (_manualStepIndex <= 0)
            {
                UpdateButtonStates();
                return;
            }
            RunSequentialStep(_manualStepIndex - 1);
        }

        private void OnStopClick(object sender, EventArgs e)
        { if (IsDesign) return; _sequence?.Stop(); }

        private void OnRecoverClick(object sender, EventArgs e)
        {
            if (IsDesign) return;
            if (_sequence == null) return;

            // 순차 수동 모드에서는 Recover 시 첫 Step(0)으로 되돌림
            if (_enforceSequential && _startSingle != null)
            {
                try
                {
                    // Error 상태에서 Recover(0) 호출 → 내부적으로 Running(or Idle→Start) 전환 후
                    // 지정 Step(0)으로 포인터 이동 (구현체 Recover 로직에 따름)
                    _sequence.Recover(0);
                }
                catch { _sequence.Recover(); }

                // 사용자 Manual ▶ 로 다시 0번 Step 실행할 수 있도록 내부 인덱스 초기화
                _manualStepIndex = -1;
                try { _lstStep.ClearSelected(); } catch { }
                UpdateButtonStates();
                return;
            }

            _sequence.Recover();
        }
        #endregion

        // 연속으로 실행 됨.
        //#region Button Actions
        //    private void OnManualClick( object sender, EventArgs e )
        //{
        //    if ( IsDesign ) return;
        //    if ( _sequence == null ) return;

        //    // 순차 강제 + StartSingle 지원되는 경우: 다음 Step 실행
        //    if ( _enforceSequential && _startSingle != null )
        //    {
        //        if ( !_firstConfirmShown )
        //        {
        //            try
        //            {
        //                var selKey = _cboSequence != null ? _cboSequence.SelectedItem as string : null;
        //                var msg = "수동 순차 실행을 시작합니다.\r\n(Manual ▶ = 다음 Step, Back = 이전 Step)\r\n" +
        //                          ( string.IsNullOrWhiteSpace( selKey ) ? "" : $"시퀀스: {selKey}" );
        //                var dr = MessageBox.Show( msg, "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question );
        //                if ( dr != DialogResult.Yes ) return;
        //                _firstConfirmShown = true;
        //            }
        //            catch { }
        //        }

        //        if ( _sequence.IsRunning || _sequence.IsPaused )
        //        {
        //            // 실행 도중이면 Stop
        //            _sequence.Stop();
        //            return;
        //        }

        //        // 첫 클릭이면 0번부터
        //        if ( _manualStepIndex < 0 )
        //        {
        //            _cachedStepNames = GetStepNamesInternal();
        //            if ( _cachedStepNames.Length == 0 )
        //            {
        //                MessageBox.Show( "등록된 Step 이 없습니다.", "Manual", MessageBoxButtons.OK, MessageBoxIcon.Information );
        //                return;
        //            }
        //            RunSequentialStep( 0 );
        //        }
        //        else
        //        {
        //            if ( _manualStepIndex >= LastStepIndex )
        //            {
        //                MessageBox.Show( "마지막 Step 입니다.", "Manual", MessageBoxButtons.OK, MessageBoxIcon.Information );
        //                UpdateButtonStates();
        //                return;
        //            }
        //            RunSequentialStep( _manualStepIndex + 1 );
        //        }
        //        return;
        //    }

        //    // ===== 기존 동작 (순차 강제 아님) =====
        //    try
        //    {
        //        var selKey = _cboSequence != null ? _cboSequence.SelectedItem as string : null;
        //        var msg = "어떤 시퀀스 진행하시겠습니까?" + ( string.IsNullOrWhiteSpace( selKey ) ? "" : "\r\n현재 선택: " + selKey );
        //        var dr = MessageBox.Show( msg, "시퀀스 실행 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question );
        //        if ( dr != DialogResult.Yes ) return;
        //    }
        //    catch { }

        //    if ( _sequence.IsRunning || _sequence.IsPaused )
        //    {
        //        _sequence.Stop();
        //        return;
        //    }

        //    if ( _startSingle != null && TryGetSelectedSingleStep( out var singleStep ) )
        //    {
        //        if ( _startSingle( singleStep ) ) UpdateButtonStates();
        //        return;
        //    }

        //    _sequence.Start();
        //    UpdateButtonStates();
        //}

        //private void OnBackClick( object sender, EventArgs e )
        //{
        //    if ( IsDesign ) return;
        //    if ( _sequence == null ) return;
        //    if ( !_enforceSequential || _startSingle == null ) return;
        //    if ( _sequence.IsRunning || _sequence.IsPaused ) return;
        //    if ( _manualStepIndex <= 0 )
        //    {
        //        UpdateButtonStates();
        //        return;
        //    }
        //    RunSequentialStep( _manualStepIndex - 1 );
        //}

        //private void OnStopClick( object sender, EventArgs e )
        //{ if ( IsDesign ) return; _sequence?.Stop(); }

        //private void OnRecoverClick( object sender, EventArgs e )
        //{ if ( IsDesign ) return; _sequence?.Recover(); }
        //#endregion

        #region UI Helpers
        private void UpdateButtonStates()
        {
            if ( IsDesign )
            {
                _btnManual.Enabled = true; _btnStop.Enabled = true; _btnRecover.Enabled = true;
                if ( _btnBack != null ) _btnBack.Enabled = true;
                return;
            }
            if ( _sequence == null )
            {
                _btnManual.Enabled = _btnStop.Enabled = _btnRecover.Enabled = false;
                if ( _btnBack != null ) _btnBack.Enabled = false;
                return;
            }

            _btnStop.Enabled = _sequence.IsRunning || _sequence.IsPaused || _sequence.IsError;
            _btnRecover.Enabled = _sequence.IsError;

            if ( _enforceSequential && _startSingle != null )
            {
                // 시퀀스 실행 중에는 Forward/Back 둘 다 막음
                bool busy = _sequence.IsRunning || _sequence.IsPaused || _sequence.IsError;
                if ( _cachedStepNames == null || _cachedStepNames.Length == 0 )
                    _cachedStepNames = GetStepNamesInternal();

                bool hasMore = ( _manualStepIndex < LastStepIndex );
                _btnManual.Enabled = !busy && hasMore;
                if ( _btnBack != null )
                    _btnBack.Enabled = !busy && _manualStepIndex > 0;
            }
            else
            {
                _btnManual.Enabled = !_sequence.IsRunning && !_sequence.IsPaused && !_sequence.IsError;
                if ( _btnBack != null ) _btnBack.Enabled = false;
            }
        }

        private void HighlightRunningStep( int stepIndex )
        {
            string name = null;
            if ( _stepIndexToName != null )
            {
                try { name = _stepIndexToName( stepIndex ); } catch { name = null; }
            }
            if ( string.IsNullOrEmpty( name ) && _sequence != null )
            {
                try
                {
                    var t = _sequence.GetType();
                    var stepEnum = t.GetNestedType( "Step", BindingFlags.NonPublic | BindingFlags.Public );
                    if ( stepEnum != null && stepEnum.IsEnum && Enum.IsDefined( stepEnum, stepIndex ) )
                        name = Enum.GetName( stepEnum, stepIndex );
                }
                catch { }
            }
            if ( string.IsNullOrEmpty( name ) ) return;

            for ( int i = 0; i < _lstStep.Items.Count; i++ )
            {
                var item = _lstStep.Items[i] as string;
                if ( item != null && item.StartsWith( StepPrefix ) )
                {
                    var nm = item.Substring( StepPrefix.Length ).Trim();
                    if ( string.Equals( nm, name, StringComparison.OrdinalIgnoreCase ) )
                    {
                        _lstStep.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void TryPopulateDesignTimeSample()
        {
            try
            {
                _lstStep.Items.Clear();
                _lstStep.Items.Add( "[DESIGN] Step preview" );
                foreach ( var s in new[] { "Init", "Move", "Action" } ) _lstStep.Items.Add( $"{StepPrefix} {s}" );
                _lstStep.Items.Add( "-" );
                _lstStep.Items.Add( "단일 Step 선택 후 Manual 실행" );
                UpdateButtonStates();
            }
            catch { }
        }

        private void SafeUI( Action a )
        { if ( IsDisposed ) return; try { if ( InvokeRequired ) BeginInvoke( a ); else a(); } catch { } }
        #endregion

        private void _panelButtons_Paint( object sender, PaintEventArgs e ) { }

       
    }
}
