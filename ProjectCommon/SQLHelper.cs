using DoranekoDB;
using System;
using System.Collections.Generic;

/// <summary>
/// プロジェクト共通（自由にカスタマイズしてください）
/// </summary>
public class SQLHelper
{
    public static string GetMargeSQL(DBMastar db, string tableName,string insertSelect)
    {
        var sql = "";
        var tbl = new TableHelper(db, tableName, TableHelper.FIELD_GET_TYPE.FieldAndTable);
        var join = "";

        foreach(var key in tbl.PrimaryKeyList)
        {
            join += $" and upd.{key}={tableName}.{key}";
        }
        if (string.IsNullOrEmpty(join) == false)
        {
            join = join.Substring(" and ".Length);
        }

        var selectSql = "";
        selectSql = " select * from " + tableName;

        sql = $@"
                    with upd as ({selectSql})
                    merge into upd
                    using ({insertSelect}) as {tableName}
                    on ({join})
                    when matched then
                            update set
                                {tbl.UpdateSetSQL}
                    when not matched then
                        insert ({tbl.InsertIntoSQL})
                        values
                        (
                             {tbl.InsertSelectSQL}
                        )   
                    when not matched by source then
                        delete
                    ;
            ";

        return sql;
    }

}

