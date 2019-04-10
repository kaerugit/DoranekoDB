using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DoranekoDB
{

    /// <summary>
    /// SQLServer用（必要ないのであれば削除可）
    /// </summary>
    public class DBSQLServer : DBMastar
    {
        public DBSQLServer()
        {
            this.ParameterKigo = "@";
            this.RenketuMoji = "+";
            this.DummyTable = "";
            this.CastSQL = "cast({0} as {1})";
        }

        protected override DbConnection GetConnection()
        {
            SqlConnection cnn = new SqlConnection(this.ConnectionString);
            return cnn;
        }

        protected override DbParameter GetParameter()
        {
            return new SqlParameter();
        }

        protected override DbCommand GetCommand()
        {
            return new SqlCommand();
        }

        protected override DbDataAdapter GetDataAdapter()
        {
            return new SqlDataAdapter();
        }

        protected override DbCommandBuilder GetCommandBuilder(DbDataAdapter dda)
        {
            return new SqlCommandBuilder((SqlDataAdapter)dda);
        }

        public override string GetSchemaSQL()
        {

            //最低こちらのデータがあればOK（CSVで表現）
            //"TABLE_NAME","COLUMN_NAME","DATA_TYPE","IS_PRIMARYKEY","IS_NULLABLE","COLUMN_DEFAULT","IS_AUTO_NUMBER","IS_IGNORE_FIELD"
            //"M_マスタ","コード","varchar(2)","1","","","",""
            //"M_マスタ","値","numeric(5,3)","","","","",""
            // ⇒　全部出力したものは　Query.csv　を参考（他DBでもこちらと同じ形式ができればベスト！）


            //参考：http://ng-notes.blogspot.jp/2011/03/sql-server.html
            //TABLE_DESCRIPTION(LOGICAL_COLUMN_NAME)はドキュメント用（改行があれば論理名とみなす フリーソフトSQL Mk - 2の仕様に近づけてみる）

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("SELECT ");
            sb.Append("    TABLE_NAME ");
            sb.Append("  , CAST(CASE WHEN CHARINDEX(CHAR(13) + CHAR(10),TABLE_DESCRIPTION) = 0 THEN ");
            sb.Append("       ISNULL(TABLE_DESCRIPTION,TABLE_NAME) ");
            sb.Append("    ELSE ");
            sb.Append("       REPLACE( ");
            sb.Append("         REPLACE( ");
            sb.Append("             SUBSTRING(TABLE_DESCRIPTION,1,CHARINDEX(CHAR(13) + CHAR(10),TABLE_DESCRIPTION)) ");
            sb.Append("         ,CHAR(13),'')");
            sb.Append("       ,CHAR(10),'')");
            sb.Append("    END AS NVARCHAR(4000)) AS LOGICAL_TABLE_NAME ");
            sb.Append("  , CAST(CASE WHEN CHARINDEX(CHAR(13) + CHAR(10),TABLE_DESCRIPTION) = 0 THEN ");
            sb.Append("       TABLE_DESCRIPTION ");
            sb.Append("    ELSE ");
            sb.Append("       SUBSTRING(TABLE_DESCRIPTION,CHARINDEX(CHAR(13) + CHAR(10),TABLE_DESCRIPTION)+LEN(CHAR(13) + CHAR(10)),LEN(TABLE_DESCRIPTION)) ");
            sb.Append("    END AS NVARCHAR(4000)) AS TABLE_DESCRIPTION ");
            sb.Append("  , IS_PRIMARYKEY ");
            sb.Append("  , IS_AUTO_NUMBER ");
            sb.Append("  , COLUMN_NAME ");
            sb.Append("  , DATA_TYPE +  ");
            sb.Append("    CASE WHEN ISNULL(DATA_SIZE, '') = '' ");
            sb.Append("    THEN '' ");
            sb.Append("    ELSE '(' + DATA_SIZE + ')' ");
            sb.Append("    END AS DATA_TYPE ");
            sb.Append("  , CAST(CASE WHEN CHARINDEX(CHAR(13) + CHAR(10),COLUMN_DESCRIPTION) = 0 THEN ");
            sb.Append("       ISNULL(COLUMN_DESCRIPTION,COLUMN_NAME) ");
            sb.Append("    ELSE ");
            sb.Append("       REPLACE( ");
            sb.Append("         REPLACE( ");
            sb.Append("             SUBSTRING(COLUMN_DESCRIPTION,1,CHARINDEX(CHAR(13) + CHAR(10),COLUMN_DESCRIPTION)) ");
            sb.Append("         ,CHAR(13),'')");
            sb.Append("       ,CHAR(10),'')");
            sb.Append("    END AS NVARCHAR(4000)) AS LOGICAL_COLUMN_NAME ");
            sb.Append("  , CAST(CASE WHEN CHARINDEX(CHAR(13) + CHAR(10),COLUMN_DESCRIPTION) = 0 THEN ");
            sb.Append("       COLUMN_DESCRIPTION ");
            sb.Append("    ELSE ");
            sb.Append("             SUBSTRING(COLUMN_DESCRIPTION,CHARINDEX(CHAR(13) + CHAR(10),COLUMN_DESCRIPTION)+LEN(CHAR(13) + CHAR(10)),LEN(COLUMN_DESCRIPTION)) ");
            sb.Append("    END AS NVARCHAR(4000)) AS COLUMN_DESCRIPTION ");
            sb.Append("  , IS_NULLABLE ");
            sb.Append("  , COLUMN_DEFAULT ");
            sb.Append("  , IS_IGNORE_FIELD ");

            sb.Append("FROM ");
            sb.Append("  ( ");
            sb.Append("    SELECT ");
            sb.Append("      [COL].TABLE_NAME AS TABLE_NAME                               /*[テーブル名]*/ ");
            sb.Append("      , cast(TEP.value as NVARCHAR(4000)) AS TABLE_DESCRIPTION     /*[テーブル説明]*/ ");
            sb.Append("      , [COL].ORDINAL_POSITION AS ORDINAL_POSITION                 /*[列番]*/ ");
            sb.Append("      , CASE ");
            sb.Append("        WHEN [KEY].CONSTRAINT_NAME IS NULL THEN ");
            sb.Append("         '' ");
            sb.Append("        ELSE ");
            sb.Append("         '1' ");
            sb.Append("        END AS IS_PRIMARYKEY                                       /*[主キー]*/ ");
            sb.Append("      , CASE ");
            sb.Append("        WHEN COLUMNPROPERTY( ");
            sb.Append("          OBJECT_ID( ");
            sb.Append("            QUOTENAME([COL].TABLE_SCHEMA) + '.' + QUOTENAME([COL].TABLE_NAME) ");
            sb.Append("          ) ");
            sb.Append("          , [COL].COLUMN_NAME ");
            sb.Append("          , 'IsIdentity' ");
            sb.Append("        ) = 0 ");
            sb.Append("        THEN '' ");
            sb.Append("        ELSE '1' ");
            sb.Append("        END AS IS_AUTO_NUMBER                                        /*[自動発番]*/ ");
            sb.Append("      , [COL].COLUMN_NAME AS COLUMN_NAME                             /*[列名]*/ ");
            sb.Append("      , [COL].DATA_TYPE AS DATA_TYPE                                 /*[データ型]*/ ");
            sb.Append("      , CAST(FEP.value as NVARCHAR(4000)) AS COLUMN_DESCRIPTION      /*[カラム説明]*/ ");
            sb.Append("      , CASE ");
            sb.Append("        WHEN [COL].DATA_TYPE LIKE '%char' or [COL].DATA_TYPE LIKE 'varbinary%' ");
            sb.Append("        THEN CASE [COL].CHARACTER_MAXIMUM_LENGTH ");
            sb.Append("          WHEN - 1 THEN 'MAX' ");
            sb.Append("          WHEN NULL THEN '' ");
            sb.Append("          ELSE ISNULL( ");
            sb.Append("            CONVERT(VARCHAR (10),[COL].CHARACTER_MAXIMUM_LENGTH) ");
            sb.Append("            , '' ");
            sb.Append("          ) ");
            sb.Append("          END ");
            sb.Append("        WHEN [COL].DATA_TYPE IN ('decimal', 'numeric') ");
            sb.Append("        THEN CONVERT(VARCHAR, [COL].NUMERIC_PRECISION) + ',' + CONVERT(VARCHAR, [COL].NUMERIC_SCALE) ");
            sb.Append("        ELSE '' /*ISNULL(CONVERT(VARCHAR,[COL].NUMERIC_PRECISION), '---')*/ ");
            sb.Append("        END AS DATA_SIZE /*[データサイズ]*/ ");
            sb.Append("      , CASE ");
            sb.Append("        WHEN [COL].IS_NULLABLE = 'NO' ");
            sb.Append("          THEN '' ");
            sb.Append("          ELSE '1' ");           //nullを挿入可
            sb.Append("        END AS IS_NULLABLE /*[NULL許容]*/ ");
            sb.Append("      , REPLACE(REPLACE(ISNULL([COL].COLUMN_DEFAULT, ''),'(',''),')','') AS COLUMN_DEFAULT           /*[DEFAULT]*/ ");

            //自動的にセットされるフィールド(insert文などで無視してよいフィールド)
            sb.Append("     , CASE WHEN LOWER([COL].DATA_TYPE) in ('timestamp') THEN ");
            sb.Append("     　          '1'  ");
            sb.Append("     　      ELSE  ");
            sb.Append("     　          ''  ");
            sb.Append("     　END AS IS_IGNORE_FIELD ");

            sb.Append("    FROM ");
            sb.Append("      INFORMATION_SCHEMA.COLUMNS [COL] ");
            sb.Append("      LEFT OUTER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE [KEY] ");
            sb.Append("        ON  [COL].TABLE_NAME = [KEY].TABLE_NAME ");
            sb.Append("        AND [COL].COLUMN_NAME = [KEY].COLUMN_NAME ");
            sb.Append("      /* テーブルの説明の取得 */ ");
            sb.Append("      LEFT OUTER JOIN ( ");
            sb.Append("        SELECT ");
            sb.Append("          * ");
            sb.Append("        FROM ");
            sb.Append("          sys.extended_properties ");
            sb.Append("        WHERE ");
            sb.Append("              MINOR_ID = 0 ");
            sb.Append("          AND NAME = 'MS_Description' ");
            sb.Append("      ) AS TEP ");
            sb.Append("        ON OBJECT_ID([COL].TABLE_NAME) = TEP.major_id ");
            sb.Append("      /* フィールドの説明の取得 */ ");
            sb.Append("      LEFT OUTER JOIN sys.extended_properties AS FEP ");
            sb.Append("        ON OBJECT_ID([COL].TABLE_NAME) = FEP.major_id ");
            sb.Append("        AND [COL].ORDINAL_POSITION = FEP.minor_id ");
            sb.Append("        AND [FEP].NAME = 'MS_Description' ");
            sb.Append("    WHERE ");
            sb.Append("      1 = 1 ");
            sb.Append("  ) AS SUB ");

            sb.Append("ORDER BY ");
            sb.Append("    TABLE_NAME ");
            sb.Append("  , ORDINAL_POSITION ");

            return sb.ToString();
        }
        public override void GetSqlTypeToDbType(string sqlTypeString, out DbType pdbType, out int size, out decimal maxValue, out decimal minValue)
        {
            int kakkoindex = sqlTypeString.IndexOf("(");
            string sqlTypeCheckString = sqlTypeString;
            if (kakkoindex >= 0)
            {
                sqlTypeCheckString = sqlTypeCheckString.Substring(0, kakkoindex);
            }

            size = 0;
            maxValue = 0;
            minValue = 0;

            //DBMastar.cs  changeValueメソッドで でこちらで判定された型で範囲チェックを行っている
            switch (sqlTypeCheckString.ToLower())
            {
                case "numeric":
                    pdbType = DbType.Decimal;
                    break;
                case "money":
                    pdbType = DbType.Decimal;
                    break;
                case "smallmoney":
                    pdbType = DbType.Decimal;
                    break;
                case "smallint":
                    pdbType = DbType.Int16;
                    break;
                case "int":
                    pdbType = DbType.Int32;
                    break;
                case "bigint":
                    pdbType = DbType.Int64;
                    break;
                case "date":
                    pdbType = DbType.Date;
                    break;
                case "datetime":
                    pdbType = DbType.DateTime;
                    break;
                case "datetime2":
                    pdbType = DbType.DateTime2;
                    break;
                case "varbinary":
                case "binary":
                case "image":
                    pdbType = DbType.Binary;
                    break;
                case "timestamp":
                    pdbType = DbType.Binary;
                    break;
                case "char":
                    pdbType = DbType.AnsiStringFixedLength;
                    break;
                case "nchar":
                    pdbType = DbType.StringFixedLength;
                    break;
                case "varchar":
                    pdbType = DbType.AnsiString;
                    break;
                case "bit":
                    pdbType = DbType.Boolean;
                    break;
                case "float":
                    pdbType = DbType.Double;
                    break;
                case "xml":
                    pdbType = DbType.Xml;
                    break;
                default:   //nvarchar など
                           //その他
                    pdbType = DbType.String;
                    break;
            }

            if (kakkoindex >= 0)
            {
                string sizeString = sqlTypeString.Substring(kakkoindex);
                if (string.IsNullOrEmpty(sizeString.Trim()))
                {
                }
                else
                {
                    sizeString = sizeString.Replace("(", "").Replace(")", "").Replace(" ", "");
                    string[] sizeSplit = sizeString.Split(',');
                    if (sizeSplit.Length == 0)
                    {
                    }
                    else if (sizeSplit.Length == 1)
                    {
                        //varbinary(max) とかは無視
                        if (pdbType == DbType.String)
                        { 
                            if (string.IsNullOrEmpty(sizeSplit[0]) == false)
                            {
                                int outValue;
                                if (int.TryParse(sizeSplit[0], out outValue))
                                {
                                    size = outValue;
                                }

                                if (size <= 0)       //nvarchar(max)
                                {
                                    //size = 8000; //無制限とする
                                }
                            }
                        }
                    }
                    else
                    {
                        //decimal(5, 3) など
                        if (string.IsNullOrEmpty(sizeSplit[0]) == false && string.IsNullOrEmpty(sizeSplit[1]) == false)
                        {
                            var seisu = int.Parse(sizeSplit[0]) - int.Parse(sizeSplit[1]);
                            maxValue = decimal.Parse("".PadLeft(seisu, '9') + "." + "".PadLeft(int.Parse(sizeSplit[1]), '9'));
                            minValue = maxValue * -1;
                        }
                    }
                }
            }
        }
    }


}
