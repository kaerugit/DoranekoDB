using System.Collections.Generic;
using System.Data;

namespace DoranekoDB
{
    public class DBFieldData
    {
        public enum SQL_UPDATE_TYPE
        {
            /// <summary>INSERT(DbTypeの値と違う場合、nullに変換されます)</summary>

            INSERT,
            /// <summary>UPDATE(DbTypeの値と違う場合、nullに変換されます)</summary>

            UPDATE,
            WHERE
        }

        public struct FieldData
        {
            /// <summary>null可能の場合：true</summary>
            public bool IsNullable;
            /// <summary>フィールドタイプ</summary>
            public DbType DbType;
            /// <summary>サイズ</summary>

            public int Size;
            /// <summary>最大値</summary>

            public decimal MaxValue;
            /// <summary>最小値</summary>

            public decimal MinValue;
            /// <summary>デフォルト値</summary>
            public string DefaultValue;

        }

        /// <summary>メモリ上のデータベースの情報</summary>
        public static System.Collections.Concurrent.ConcurrentDictionary<string, FieldData> FieldMember = new System.Collections.Concurrent.ConcurrentDictionary<string, FieldData>();

        //public static DataTable FieldDataMember;
        /// <summary>メモリ上のデータベースの情報（全て）</summary>
        /// <remarks>初期に必ずセットしてください</remarks>
        public static List<FieldDataMember> FieldDataMemberList;

        /// <summary>
        /// TEMPTable(一時)作成時のテンプレートSQL
        /// </summary>
        /// <remarks>
        /// データを更新したい場合は主キーが必要なので追加しておいたほうが無難
        /// {0} にテーブル名
        /// {1} にフィールド名　がセットされます。
        /// サンプル
        /// " create table {0} ({1},TEMP_AUTO_NUMBER int identity not null,primary key (TEMP_AUTO_NUMBER))"
        /// </remarks>
        public static string CreateTableSQL = "";

        /// <summary>【共通用】定義(Dbtable)に無いものは代用の定義を使う場合にセット キー：代用の定義　　値：変換の定義(Dbtable.～)　</summary>
        /// <remarks>
        /// select *, '追加' as 追加項目　←　を実行した場合、追加項目に定義が紐づかないので
        /// 定義を紐づかせるために設定が必要となる
        /// </remarks>
        public static Dictionary<string, string> DummyFieldChangePublic { get; set; } = null;

    }

    public class FieldDataMember
    {
        public string TABLE_NAME { get; set; }
        public string IS_PRIMARYKEY { get; set; }
        public string IS_AUTO_NUMBER { get; set; }
        public string COLUMN_NAME { get; set; }
        public string DATA_TYPE { get; set; }
        public string IS_NULLABLE { get; set; }
        public string COLUMN_DEFAULT { get; set; }
        public string IS_IGNORE_FIELD { get; set; }
    }
}
