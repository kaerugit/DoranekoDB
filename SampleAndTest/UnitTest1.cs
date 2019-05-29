using DoranekoDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

//�����@�J�n����ɂ́@���j���[�@�e�X�g�[���s(or �f�o�b�O)-�S�Ẵe�X�g

namespace SampleAndTest
{


    #region "���ԗp�@https://stackoverflow.com/questions/9210281/how-to-set-the-test-case-sequence-in-xunit�@�Q�l"

    [AttributeUsage(AttributeTargets.Method)]
    public class TestPriorityAttribute : Attribute
    {
        public TestPriorityAttribute(int priority)
        {
            this.Priority = priority;
        }

        public int Priority { get; }
    }

    public class PriorityOrderer : ITestCaseOrderer
    {

        //[TestCaseOrderer("SampleAndTest.PriorityOrderer", "SampleAndTest")]�@�̌����`

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var sortedMethods = new Dictionary<int, TTestCase>();

            foreach (var testCase in testCases)
            {
                var attributeInfo = testCase.TestMethod.Method.GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName)
                    .SingleOrDefault();
                if (attributeInfo != null)
                {
                    var priority = attributeInfo.GetNamedArgument<int>("Priority");
                    sortedMethods.Add(priority, testCase);
                }
            }

            return sortedMethods.OrderBy(x => x.Key).Select(x => x.Value);
        }
    }
    #endregion

    [TestCaseOrderer("SampleAndTest.PriorityOrderer", "SampleAndTest")] //PriorityOrderer
    public class UnitTest1
    {
        enum DATA
        {
            one = 1
        }

        public UnitTest1()
        {

            //�K��1����s
            CommonData.InitDB();

        }

        [Fact(DisplayName = "01�f�[�^�x�[�X�����ݒ�"), TestPriority(1)]
        public void DBInit()
        {
            //CommonData.InitDB();

            Assert.NotEqual(0, (long)DBFieldData.FieldDataMemberList.Count);
        }

        [Fact(DisplayName = "02�f�[�^�ǉ��E�X�V(SQL)"), TestPriority(2)]
        public void DBSQL()
        {
            var db = CommonData.GetDB();

            //�ő�̔ԍ����擾
            var dt = db.GetDataTable($@"
                        select 
                            isnull(max({DbTable.T_TEST.�e�X�g�p�ԍ�.Name}),0) as maxdata 
                        from
                            {DbTable.T_TEST.Name}
                ");

            int maxData = 0;
            if (dt.Rows.Count > 0)
            {
                maxData = int.Parse(dt.Rows[0]["maxdata"].ToString());
            }

            db.BeginTransaction();

            //�e�X�g�p�f�[�^�̑}��
            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name))      //�D�݂ŁE�E
            {
                //�����ʏ�̑}���@SQL���𗘗p���Ă̑}�������@�����Ⴒ�������Ă܂����A���_�@db.Execute(�@�Ŏ��s�����sql�����m�F����̂������Ǝv���܂��E�E

                //SQL�̃C���[�W
                //tbl.Field �̃L�[ insert into �e�[�u����(��)
                //tbl.Field �̒l   select ��

                tbl.AddParameter(DbTable.T_TEST.TEXTDATA.Name, "�e�X�g" + System.DateTime.Now.ToString());
                //���������Ӗ�
                //tbl.Field[DbTable.T_TEST.TEXTDATA.Name] = db.AddInsertParameter(DbTable.T_TEST.TEXTDATA.Name, "�e�X�g");
                //tbl[DbTable.T_TEST.TEXTDATA.Name] = db.AddInsertParameter(DbTable.T_TEST.TEXTDATA.Name, "�e�X�g");

                maxData++;
                tbl.AddParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, maxData);
                tbl.AddParameter(DbTable.T_TEST.�t���O.Name, true);

                tbl.AddParameter(DbTable.T_TEST.TEXTMAX.Name, DATA.one);

                //nvarchar(50)��51�������ꂽ�ꍇ�̃e�X�g�i�������DB�N���X�ł͓���Ȃ������͋�؂��ăZ�b�g�j��"��"������
                tbl.AddParameter(DbTable.T_TEST.TEXTDATA.Name, "12345678901234567890123456789012345678901234567890��");
                //���l�����I�[�o�[�����ꍇ�̃e�X�g�i�������DB�N���X�ł�null(or �f�t�H���g�l)���Z�b�g�j
                tbl.AddParameter(DbTable.T_TEST.���z.Name, double.MaxValue);

                //DbTable�ɒ�`����Ă��Ȃ��t�B�[���h��OK  (�t�B�[���h�^�C�v�́A������q�؂��邱��)
                //tbl.Field["�Ă���"] = db.AddParameter(DbTable.T_TEST.�t���O.Name, "aaaa");

                //(InsertIntoSQL�v���p�e�B���g����)�X�V������CommonData.GetDB(InsertUpdateDataParameter)���Ŏ����I�ɍX�V
                db.Execute($@"
                        insert into {DbTable.T_TEST.Name}
                                ({tbl.InsertIntoSQL})
                        values 
                                ({tbl.InsertSelectSQL}) 
                    ");

            }

            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name, TableHelper.FIELD_GET_TYPE.Field))
            {
                Assert.True(tbl.Field.Keys.Count != 0);

                //����SQL���𗘗p���Ă̑}������
                //tbl.Field�@�ɂ� Autonumber�i�����}��off�j���������f�[�^���Z�b�g����Ă��܂��B(�擾�������ꍇ�́@TableHelper�̈����Q��)
                var oldmaxkbn = maxData;

                maxData++;
                tbl.AddParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, maxData);
                //sql��(substring�̋L�q)���\
                tbl[DbTable.T_TEST.TEXTDATA.Name] = "substring(" + DbTable.T_TEST.TEXTDATA.Name + ",1,10)";     //tbl.Field[] �̏ȗ��`
                tbl[DbTable.T_TEST.���z.Name] = "1000";

                //���ݒ肳��Ă��Ȃ��f�[�^�͌��f�[�^�̒l��insert
                db.Execute($@"
                        insert into {DbTable.T_TEST.Name}
                                ({tbl.InsertIntoSQL})
                        select 
                                {tbl.InsertSelectSQL}
                        from 
                                {DbTable.T_TEST.Name}
                        where 
                                {db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, oldmaxkbn)}

                    ");


                //���������悤�ȃe�[�u���ɃR�s�[�iT_TEST�̃f�[�^�ɂ�����T_TEST_COPY�ɂȂ��f�[�^���R�s�[�j����
                tbl.FieldGetType = TableHelper.FIELD_GET_TYPE.FieldAndTable;
                tbl.Reset();

                //tbl.Field[DbTable.T_TEST.TEXTDATA.Name] =  "T_TEST.TEXTDATA" �̃f�[�^�������Ă��܂��B
                //�e�[�u�������K�v�Ȏ��́@FullName�@���g�p
                tbl[DbTable.T_TEST.ID_AUTO.Name] = DbTable.T_TEST.ID_AUTO.FullName;   //T_TEST_COPY��ID_AUTO�̓I�[�g�i���o�[�ł͂Ȃ�
                tbl.AddParameter(DbTable.T_TEST.TEXTFIX.Name, System.DBNull.Value);

                var sql =
                    $@"
                        insert into {DbTable.T_TEST_COPY.Name}
                                ({tbl.InsertIntoSQL})
                        select 
                                {tbl.InsertSelectSQL}
                        from 
                                {DbTable.T_TEST.Name}
                                left join   {DbTable.T_TEST_COPY.Name}
                                    on {DbTable.T_TEST.ID_AUTO.FullName} = {DbTable.T_TEST_COPY.ID_AUTO.FullName} 
                        where 
                                {db.AddWhereParameter(DbTable.T_TEST_COPY.ID_AUTO.FullName, System.DBNull.Value)}

                    ";

                db.Execute(sql);
            }


            try
            {
                db.Commit();
            }
            catch
            {
                db.Rollback();
            }

            db = CommonData.GetDB();
            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name))      //�D�݂ŁE�E
            {
                //����Update�e�X�g����
                tbl.AddParameter(DbTable.T_TEST.TEXTFIX.Name, "TEST");

                //����SQL�����������Ƃ��\
                tbl.Field[DbTable.T_TEST.���z.Name] = $@"
                            case when {db.AddWhereParameter(DbTable.T_TEST.���z.Name, 0, "<>")} then
                                {DbTable.T_TEST.���z.Name} + 100
                            else
                                {DbTable.T_TEST.���z.Name}
                            end
                            "
                     ;
                //(UpdateSetSQL���g����)�X�V�����Ȃǂ������ŃZ�b�g
                db.Execute($@"
                        update {DbTable.T_TEST_COPY.Name}
                        set 
                                {tbl.UpdateSetSQL}
                    ");
            }

            db.Close();

        }


        [Fact(DisplayName = "03�ʏ��SELECT"), TestPriority(3)]
        public void Select1()
        {

            //��01���珇�ԂŎ��s���Ȃ��ƃG���[�ɂȂ�̂Œ���

            var db = CommonData.GetDB();


            //�ő�̔ԍ����擾
            var dt = db.GetDataTable($@"
                        select 
                            isnull(max({DbTable.T_TEST.�e�X�g�p�ԍ�.Name}),0) as maxdata 
                        from
                            {DbTable.T_TEST.Name}
                ");

            int maxData = 0;
            if (dt.Rows.Count > 0)
            {
                maxData = int.Parse(dt.Rows[0]["maxdata"].ToString());
            }

            //�ʏ��where
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, maxData)}
                    
               ");

            if (dt.Rows.Count > 0)
            {
                //��U�G���[�ɂȂ邪�A���s���Ԃ̊֌W��2��ڈȍ~�͎��s�����i�͂��j
                Assert.Equal(1, (long)dt.Rows.Count);
            }



            //����in�偡��
            var lst = new List<int>() { maxData, maxData - 1 };
            //AddWhereParameter �̏ꏊ�Ł@�e�X�g�p�ԍ� in (xx,yy) �̃f�[�^���쐬
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}  
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, lst)}
                    
               ");
            Assert.True(dt.Rows.Count > 1);


            //����enum�̃e�X�g����
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTMAX.Name, DATA.one)}
               ");

            Assert.True(dt.Rows.Count > 1);

            //�����ȏち��
            //�e�X�g�p�ԍ�>=xx 
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, maxData, DBMastar.WHERE_FUGO.Ijyo)}
                    
               ");

            Assert.Equal(1, dt.Rows.Count);


            //����Like�̃e�X�g����
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTDATA.Name, "1234", DBMastar.WHERE_FUGO.Like)}
               ");

            Assert.True(dt.Rows.Count > 1);

            //�����t���O�̃e�X�g����
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.�t���O.Name, null)}
               ");

            Assert.True(dt.Rows.Count > 1);

        }


        [Fact(DisplayName = "04�ʏ��SELECT(sql���̎g���܂킵)"), TestPriority(4)]
        public void Select2()
        {

            var db = CommonData.GetDB();

            var sql = $@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    where
                        { db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, 0, DBMastar.WHERE_FUGO.Not)}
            ";


            var count1 = db.GetDataTable(sql).Rows.Count;

            //����sql��(�ϐ�sql)���g���܂킵
            var sqlSub = $@"
                    select 
                    from 
                    ({sql}) as sub
                    where
                        { db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, 0, DBMastar.WHERE_FUGO.Not)}
                ";

            var count2 = db.GetDataTable(sql).Rows.Count;

            Assert.Equal(count1, count2);


            //���X�|���XNG��
            for (var i = 0; i < 100; i++)
            {
                sql = $@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    where
                        { db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, 0, DBMastar.WHERE_FUGO.Not)}
                ";

                //��������Ə�ɓ����p�����[�^�����܂�̂Ń��X�|���X�I�ɂ܂����Ȃ��Ă���
                var dt= db.GetDataTable(sql);

            }

            //���X�|���XOK��@���̐�����������
            for (var i = 0; i < 100; i++)
            {

                //����U����������s�i�����p�����[�^���N���A�j ����鎖�ɂ���đ����Ȃ�܂��B
                db.ClearSQLParameter();

                sql = $@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    where
                        { db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, 0, DBMastar.WHERE_FUGO.Not)}
                ";

                var dt = db.GetDataTable(sql);

            }


        }

        const int MAX_DATA = 10000;

        [Fact(DisplayName = "05�f�[�^�ǉ��E�X�V(Dataset)"), TestPriority(5)]
        public void DBDataSet()
        {
            var db = CommonData.GetDB();
            db.ConnectionAutoClose = false;

            db.BeginTransaction();

            //�����V�K�ǉ�����
            db.OpenDataSet($@"
                    select 
                        * 
                    from 
                        {DbTable.T_TEST.Name}
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.ID_AUTO.Name, System.DBNull.Value)}
                ");


            var dt = db.DataSet.Tables[0];
            var nowString = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            //100���f�[�^���쐬
            for (var i = 0; i < MAX_DATA; i++)
            {
                var drNew = dt.NewRow();

                drNew[DbTable.T_TEST.TEXTMAX.Name] = nowString;
                drNew[DbTable.T_TEST.TEXTDATA.Name] = "�}��" + (i + 1).ToString();
                drNew[DbTable.T_TEST.���z.Name] = i * 1000;
                dt.Rows.Add(drNew);
            }

            //�X�V(�X�V������ CommonData.GetDB InsertUpdateDataDataRow ���ŏ���)
            db.UpdateDataSet();

            //�����X�V����
            db.OpenDataSet($@"
                    select 
                         * 
                    from 
                        {DbTable.T_TEST.Name}
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTDATA.Name, "�}��", DBMastar.WHERE_FUGO.Like)}
                ", "�Ȃ܂���");


            dt = db.DataSet.Tables["�Ȃ܂���"];


            foreach (System.Data.DataRow dr in dt.Rows)
            {


                dr[DbTable.T_TEST.TEXTDATA.Name] = dr[DbTable.T_TEST.TEXTDATA.Name].ToString() + "�X�V";
                //dr[DbTable.T_TEST.TEXTDATA.Name] = dr[DbTable.T_TEST.TEXTDATA.Name].ToString() + "�X�V";
                dr[DbTable.T_TEST.���z.Name] = decimal.Parse(dr[DbTable.T_TEST.���z.Name].ToString()) + 100;
            }

            //�X�V�i�����w��Ł@�Ώۃe�[�u���̂ݍX�V���j
            db.UpdateDataSet("�Ȃ܂���");


            try
            {
                db.Commit();
            }
            catch
            {
                db.Rollback();
            }


            //
            var sql = $@"
                    select 
                         * 
                        ,{DbTable.T_TEST.�t���O.Name} as ��`�t�B�[���h������ɒǉ�
                    from 
                        {DbTable.T_TEST.Name}
                    where
                             {db.AddWhereParameter(DbTable.T_TEST.���z.Name, 0, ">=")}
                        and  {db.AddWhereParameter(DbTable.T_TEST.TEXTMAX.Name, nowString)}
                ";

            var dtSelect = db.GetDataTable(sql);

            //�Ō�̃f�[�^���폜
            dtSelect.Rows.Remove(dtSelect.Rows[dtSelect.Rows.Count - 1]);

            //�f�[�^��ύX
            var drSelect = dtSelect.NewRow();
            drSelect[DbTable.T_TEST.TEXTDATA.Name] = "�����Ł[��";
            drSelect[DbTable.T_TEST.���z.Name] = 10;

            if (System.IO.File.Exists(@"C:\inetpub\wwwroot\iis-85.png") == true)
            {
                FileStream fs = new FileStream(@"C:\inetpub\wwwroot\iis-85.png", FileMode.OpenOrCreate, FileAccess.Read);
                byte[] MyData = new byte[fs.Length];
                fs.Read(MyData, 0, System.Convert.ToInt32(fs.Length));

                fs.Close();

                drSelect[DbTable.T_TEST.�o�C�i���[.Name] = MyData;
            }
            drSelect[DbTable.T_TEST.�t���O.Name] = DBMastar.DBTrueValue;

            //true false(bool)�̕ϊ� ��null�̏ꍇ�G���[�ɂȂ�̂Ńv���W�F�N�g�p�ɐ�p�֐�����Ă��悢�����E�E
            Assert.True((bool)(drSelect[DbTable.T_TEST.�t���O.Name]));                     //�ϊ������� true����ł�OK(������null�̏ꍇ�G���[)
            Assert.True(drSelect.Field<bool>(DbTable.T_TEST.�t���O.Name));                 //nget ���@System.Data.DataSetExtensions�@�̎Q�Ƃ��K�v�@��������������Ƃ킩��ɂ����H�H
            //Assert.Equal(drSelect[DbTable.T_TEST.�t���O.Name],DBMastar.DBTrueValue);       //������ł�OK

            drSelect["��`�t�B�[���h������ɒǉ�"] = DBMastar.DBTrueValue;
            dtSelect.Rows.Add(drSelect);


            dtSelect.Rows[0][DbTable.T_TEST.���͓�.Name] = System.DateTime.Now.Date;
            dtSelect.Rows[0][DbTable.T_TEST.���z.Name] = 888;


            //json�ɕϊ�(web api�Ȃǂő��M)
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dtSelect);


            //json����DataTable�ɕϊ�(web api�ȂǂŎ�M)

            //json����̖߂�(SQLServer2016 ���� json �����̂܂�table���ł���̂ł�������g�p�����ق����悢�Ǝv���@hint OPENJSON�֐� )
            var dataList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonString);
            //var dtInput = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(jsonString);

            var tempTableName = "##temp" + nowString;

            if (dataList.Count > 0)
            {
                var dummyFieldChange = new Dictionary<string, string>()
                        {
                            { "��`�t�B�[���h������ɒǉ�", DbTable.T_TEST.�t���O.Name }
                        };


                var tbl = new TableHelper(db, DbTable.T_TEST.Name, paraDummyFieldChange: dummyFieldChange);
                //���݂̃f�[�^����Createtable���s��
                //ToList����ɂ́@using System.Linq;�@���K�v
                db.Execute(tbl.GetCreateSQL(tempTableName, dataList[0].Keys.ToList()));

                //�`�ʏ��
                if (false)
                {
                    //db.BeginTransaction();
                    db.ConnectionAutoClose = false; //##�e�[�u����������̂ŁE�E

                    db.OpenDataSet($"select * from  {tempTableName}");

                    var dtTemp = db.DataSet.Tables[0];
                    foreach (var data in dataList)
                    {
                        var dr = dtTemp.NewRow();
                        foreach (var key in data.Keys)
                        {
                            //��������t�B�[���h
                            if (tbl.IgnoreFieldList.Contains(key))
                            {
                                continue;
                            }

                            if (data[key] == null)
                            {
                                dr[key] = System.DBNull.Value;
                            }
                            else
                            {
                                //�o�C�i����̂ݓ��ʏ����i�I�u�W�F�N�g�����̂܂ܓ����ƃG���[�ɂȂ�̂ŁAbyte�f�[�^�Ƃ��ď����j
                                if (dtTemp.Columns[key].DataType == typeof(System.Byte[]))
                                {
                                    dr[key] = System.Convert.FromBase64String(data[key].ToString());
                                }
                                else
                                {
                                    dr[key] = data[key];
                                }
                            }
                        }
                        dtTemp.Rows.Add(dr);
                    }

                    db.UpdateDataSet();
                    //db.Commit();
                }


                //�`�����Łi�����Ǖ��ׂ�������܂��j
                if (false)
                {
                    var filed = tbl.InsertIntoSQL;

                    Parallel.ForEach(dataList, data =>
                    {

                        //Parallel.ForEach ���ł�db�I�u�W�F�N�g�̎g���܂킵�͌���
                        using (var dbpara = CommonData.GetDB())
                        {
                            var tblpara = new TableHelper(dbpara, DbTable.T_TEST.Name, paraDummyFieldChange: dummyFieldChange);

                            sql = $@"
                                insert into {tempTableName}
                                    ({filed})
                                select 
                                    {filed}
                                from 
                                    (
                                    {tblpara.GetSQL(data)}
                                    ) as ins
                                ";

                            dbpara.Execute(sql);
                        }
                    });
                }



                //������ł�OK
                if (false)
                {
                    var filed = tbl.InsertIntoSQL;
                    foreach (var data in dataList)
                    {

                        db.ClearSQLParameter();   //����������������s���Ȃ��ƃp�����[�^�����܂��Ă������x��

                        sql = $@"
                            insert into {tempTableName}
                                ({filed})
                            select 
                                {filed}
                            from 
                                (
                                {tbl.GetSQL(data)}
                                ) as ins
                           ";

                        db.Execute(sql);

                    }
                }

                //�^��BULK INSERT (���܂葁���Ȃ�)
                if (true)
                {
                    var insertCount = 200;
                    //var data = Enumerable.Range(0, dataList.Count);

                    var tblTemp = new TableHelper(db, DbTable.T_TEST.Name, TableHelper.FIELD_GET_TYPE.Field);
                    
                    Enumerable.Range(0, (dataList.Count / insertCount) + (dataList.Count % insertCount == 0 ? 0 : 1))
                                .Select(
                                    (index) =>
                                    {
                                        return dataList.Skip(insertCount * index).Take(insertCount).ToList();
                                    }
                               ).ToList()
                               .ForEach(
                                    (eachData) =>
                                    {
                                        var sb = new StringBuilder();
                                        sb.Append($"insert into {tempTableName}");
                                        sb.Append($"({tblTemp.InsertIntoSQL})");

                                        sb.Append($" values ");

                                        var firstFlag = true;

                                        foreach (var data in eachData)
                                        {
                                            tblTemp.Reset();

                                            foreach (var eachDic in tblTemp.Field.ToList())
                                            {
                                                object value  = System.DBNull.Value;
                                                if (data.Keys.Contains(eachDic.Key))
                                                {
                                                    value = data[eachDic.Key];
                                                }

                                                tblTemp.AddParameter(eachDic.Key, value);
                                            }

                                            if (firstFlag == true)
                                            {
                                                firstFlag = false;
                                            }
                                            else
                                            {
                                                sb.Append($",");
                                            }

                                            sb.Append($"({tblTemp.InsertSelectSQL})");

                                        }

                                        db.Execute(sb.ToString());
                                        db.ClearSQLParameter();
                                    }
                                );

                }

            }


            //�X�V
            db.BeginTransaction();
            var mergeSql = SQLHelper.GetMergeSQL(db, DbTable.T_TEST.Name, $" select * from {tempTableName}");
            db.Execute(mergeSql);
            try
            {
                db.Commit();
            }
            catch
            {
                db.Rollback();
            }

            db.Close();
        }

        [Fact(DisplayName = "06�p�����[�^�̕ۑ�"), TestPriority(6)]
        public void SaveParameter()
        {
            var db = CommonData.GetDB();

            //�p�����[�^�̕ۑ�
            db.ClearSQLParameter();

            var lst = new List<int>() { 100, 200 };

            var sql = $@"
                 select 
                   * 
                 from
                    {DbTable.T_TEST.Name} 
                 where
                        {db.AddWhereParameter(DbTable.T_TEST.���z.Name, lst, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.���z.Name, DATA.one, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.���z.Name, System.DBNull.Value, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.�t���O.Name, false, DBMastar.WHERE_FUGO.Not)}
            ";

            var datacount = db.GetDataTable(sql).Rows.Count;

            //json�I�u�W�F�N�g�ɕϊ�
            var save = Newtonsoft.Json.JsonConvert.SerializeObject(db.SQLParameter);

            //�Ⴄ�I�u�W�F�N�g�ɕ���
            var db2 = CommonData.GetDB();

            //�߂��i���̏ꍇSystem.DBNull.Value �� null�ɕϊ������̂�DBMastar�N���X�őΉ��ρj
            db2.SQLParameter = Newtonsoft.Json.JsonConvert.DeserializeObject<DBSQLParameter>(save);
            var dt2 = db2.GetDataTable(sql);

            Assert.Equal(datacount, dt2.Rows.Count);

        }

        [Fact(DisplayName = "07�p�����[�^�̃R�s�["), TestPriority(7)]
        public void CopyParameter()
        {

            //������ۑ�
            var dbSave = CommonData.GetDB();
            var whereSave = $@"
                    where
                        { dbSave.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, 1, DBMastar.WHERE_FUGO.Ijyo)}
                    ";

            var db1 = CommonData.GetDB();
            //�ۑ������p�����[�^���R�s�[(dbSave��db1)
            var where1 = db1.CopySQL(dbSave, whereSave);     //�߂�l�� whereSave

            var dt1 = db1.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    {where1}
               ");


            var db2 = CommonData.GetDB();
            var where2 = db2.CopySQL(dbSave, whereSave);     //�߂�l�� whereSave

            var dt2 = db2.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    {where2}
               ");

            Assert.Equal(dt1.Rows.Count, dt2.Rows.Count);

        }



    }


}
