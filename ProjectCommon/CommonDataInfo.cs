
using DoranekoDB;

class CommonDataInfo
{
    public static string SchemaJsonString = @"
            [{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":""1"",""IS_AUTO_NUMBER"":""1"",""COLUMN_NAME"":""ID_AUTO"",""DATA_TYPE"":""int"",""IS_NULLABLE"":"""",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TEXTMAX"",""DATA_TYPE"":""nvarchar(MAX)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TEXTDATA"",""DATA_TYPE"":""nvarchar(50)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TEXTFIX"",""DATA_TYPE"":""varchar(50)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TS"",""DATA_TYPE"":""timestamp"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""1""},{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""バイナリー"",""DATA_TYPE"":""varbinary(MAX)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""金額"",""DATA_TYPE"":""money"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":""0"",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""フラグ"",""DATA_TYPE"":""bit"",""IS_NULLABLE"":"""",""COLUMN_DEFAULT"":""0"",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""入力日"",""DATA_TYPE"":""date"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""テスト用番号"",""DATA_TYPE"":""smallint"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""更新日時"",""DATA_TYPE"":""datetime"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":""1"",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""ID_AUTO"",""DATA_TYPE"":""int"",""IS_NULLABLE"":"""",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TEXTMAX"",""DATA_TYPE"":""nvarchar(MAX)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TEXTDATA"",""DATA_TYPE"":""nvarchar(50)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TEXTFIX"",""DATA_TYPE"":""varchar(50)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TS"",""DATA_TYPE"":""timestamp"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""1""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""バイナリー"",""DATA_TYPE"":""varbinary(MAX)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""金額"",""DATA_TYPE"":""money"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""フラグ"",""DATA_TYPE"":""bit"",""IS_NULLABLE"":"""",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""入力日"",""DATA_TYPE"":""date"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""テスト用番号"",""DATA_TYPE"":""smallint"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""T_TEST_COPY"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""更新日時"",""DATA_TYPE"":""datetime"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""V_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":""1"",""COLUMN_NAME"":""ID_AUTO"",""DATA_TYPE"":""int"",""IS_NULLABLE"":"""",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""V_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TEXTMAX"",""DATA_TYPE"":""nvarchar(MAX)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""V_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TEXTDATA"",""DATA_TYPE"":""nvarchar(50)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""V_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TEXTFIX"",""DATA_TYPE"":""varchar(50)"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""V_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""TS"",""DATA_TYPE"":""timestamp"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""1""},{""TABLE_NAME"":""V_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""金額"",""DATA_TYPE"":""money"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""V_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""入力日"",""DATA_TYPE"":""date"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""},{""TABLE_NAME"":""V_TEST"",""IS_PRIMARYKEY"":"""",""IS_AUTO_NUMBER"":"""",""COLUMN_NAME"":""更新日時"",""DATA_TYPE"":""datetime"",""IS_NULLABLE"":""1"",""COLUMN_DEFAULT"":"""",""IS_IGNORE_FIELD"":""""}]
            ";
}

public class DbTable
{
    public class T_TEST
    {
        public const string Name = "T_TEST";
        public const string PrimaryKey = "ID_AUTO";


        public class ID_AUTO
        {
            public const string Name = "ID_AUTO";
            public const string FullName = "T_TEST.ID_AUTO";
        }
        public class TEXTMAX
        {
            public const string Name = "TEXTMAX";
            public const string FullName = "T_TEST.TEXTMAX";
        }
        public class TEXTDATA
        {
            public const string Name = "TEXTDATA";
            public const string FullName = "T_TEST.TEXTDATA";
        }
        public class TEXTFIX
        {
            public const string Name = "TEXTFIX";
            public const string FullName = "T_TEST.TEXTFIX";
        }
        public class TS
        {
            public const string Name = "TS";
            public const string FullName = "T_TEST.TS";
        }
        public class バイナリー
        {
            public const string Name = "バイナリー";
            public const string FullName = "T_TEST.バイナリー";
        }

        public class テスト用番号
        {
            public const string Name = "テスト用番号";
            public const string FullName = "T_TEST.テスト用番号";
        }
        public class 金額
        {
            public const string Name = "金額";
            public const string FullName = "T_TEST.金額";
        }
        public class フラグ
        {
            public const string Name = "フラグ";
            public const string FullName = "T_TEST.フラグ";
        }
        public class 入力日
        {
            public const string Name = "入力日";
            public const string FullName = "T_TEST.入力日";
        }
        public class 更新日時
        {
            public const string Name = "更新日時";
            public const string FullName = "T_TEST.更新日時";
        }
    }

    public class T_TEST_COPY
    {
        public const string Name = "T_TEST_COPY";
        public const string PrimaryKey = "ID_AUTO";


        public class ID_AUTO
        {
            public const string Name = "ID_AUTO";
            public const string FullName = "T_TEST_COPY.ID_AUTO";
        }
        public class TEXTMAX
        {
            public const string Name = "TEXTMAX";
            public const string FullName = "T_TEST_COPY.TEXTMAX";
        }
        public class TEXTDATA
        {
            public const string Name = "TEXTDATA";
            public const string FullName = "T_TEST_COPY.TEXTDATA";
        }
        public class TEXTFIX
        {
            public const string Name = "TEXTFIX";
            public const string FullName = "T_TEST_COPY.TEXTFIX";
        }
        public class TS
        {
            public const string Name = "TS";
            public const string FullName = "T_TEST_COPY.TS";
        }
        public class バイナリー
        {
            public const string Name = "バイナリー";
            public const string FullName = "T_TEST_COPY.バイナリー";
        }
        public class テスト用番号
        {
            public const string Name = "テスト用番号";
            public const string FullName = "T_TEST_COPY.テスト用番号";
        }
        public class 金額
        {
            public const string Name = "金額";
            public const string FullName = "T_TEST_COPY.金額";
        }
        public class フラグ
        {
            public const string Name = "フラグ";
            public const string FullName = "T_TEST_COPY.フラグ";
        }
        public class 入力日
        {
            public const string Name = "入力日";
            public const string FullName = "T_TEST_COPY.入力日";
        }
        public class 更新日時
        {
            public const string Name = "更新日時";
            public const string FullName = "T_TEST_COPY.更新日時";
        }
    }
}