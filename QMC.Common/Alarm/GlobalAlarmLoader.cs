using System;
using System.Collections.Generic;
using System.IO;
using QMC.Common.Alarm;

namespace QMC.Common.Alarm
{
    public class GlobalAlarmTable
    {
        // 싱글톤 패턴 적용
        private static readonly Lazy<GlobalAlarmTable> _instance = new Lazy<GlobalAlarmTable>(() => new GlobalAlarmTable());
        public static GlobalAlarmTable Instance => _instance.Value;

        // Source(UnitName) 별로 AlarmInfo 리스트를 보관
        private Dictionary<string, List<AlarmInfo>> _alarmsBySource = new Dictionary<string, List<AlarmInfo>>(StringComparer.OrdinalIgnoreCase);

        // 프로그램 시작 시 한 번 호출 (예: Program.cs 또는 Equipment 초기화 시)
        public bool LoadAlarmsFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Log.Write("AlarmLoader", $"알람 파일을 찾을 수 없습니다: {filePath}");
                    return false;
                }

                _alarmsBySource.Clear();
                var lines = File.ReadAllLines(filePath, System.Text.Encoding.GetEncoding("euc-kr"));
                //var lines = File.ReadAllLines(filePath);

                // 첫 줄(헤더) 건너뛰기
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) 
                        continue;

                    // CSV 파싱 (단순 쉼표 분리. 내용에 쉼표가 들어간다면 TextFieldParser 사용 권장)
                    //var cols = line.Split(',');
                    var cols = line.Split(new char[] { ',', '\t' });
                    if (cols.Length < 6) 
                        continue;

                    string source = cols[0].Trim();
                    int code = int.TryParse(cols[1].Trim(), out int c) ? c : 0;
                    // string key = cols[2].Trim(); // 키는 enum 매핑용으로 주석 처리(필요 시 사용)
                    string title = cols[3].Trim();
                    string cause = cols[4].Trim();
                    string grade = cols[5].Trim();

                    if (code == 0) 
                        continue;

                    var alarmInfo = new AlarmInfo
                    {
                        Source = source,
                        Code = code,
                        Title = title,
                        Cause = cause,
                        Grade = grade
                    };

                    if (!_alarmsBySource.ContainsKey(source))
                    {
                        _alarmsBySource[source] = new List<AlarmInfo>();
                    }
                    _alarmsBySource[source].Add(alarmInfo);
                }
                
                Log.Write("AlarmLoader", $"총 {_alarmsBySource.Count}개 그룹의 알람을 성공적으로 로드했습니다.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("AlarmLoader", $"알람 로드 중 에러 발생: {ex.Message}");
                return false;
            }
        }

        // 특정 Unit(Source)의 알람 목록을 반환
        public List<AlarmInfo> GetAlarmsForSource(string source)
        {
            if (_alarmsBySource.TryGetValue(source, out var alarms))
            {
                return alarms;
            }
            return new List<AlarmInfo>();
        }
    }
}