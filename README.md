# DoranekoDB
.NET Standard 2.0�`  �f�[�^�x�[�X�A�N�Z�X�p(����SQLServer�̂�)�N���X�ł��B  
�ȈՁi�^���jorm�Ƃ������ADB�̋L�q���������o���܂��B  

## Description
entity framework �Ŗ��Ȃ���΂�����̗��p�������߂��܂��B  
SQL�����S���S�������āADataTable�iDataSet�j����R�o�Ă���悤�ȃv���W�F�N�g�ɂ����߂��܂��B  
��ORM�Ƃ������A�p�����[�^���̊ȈՉ��y�уC���e���W�F���X�ɂ��e�[�u���t�B�[���h�̉��b���ő���ɗ��p�ł���N���X�ł��B  
�@��DB�̍\�����ύX�����ꍇ�́A�R���p�C���G���[�ƂȂ�܂��̂ŁADB�̕ύX�ɓK���Ή��ł���`���ƂȂ��Ă���܂��B  

## Usage
### Sample
�p�����[�^���݂�SQL�����ȈՓI�ɔ��s���܂��B  
��DbTable.�Z�Z�́@���݂̃e�[�u����`��莩���쐬�i��q�j���܂��B  
�@��(VS�̃o�[�W�����ɂ����܂���)������SQL���̂悤�ȋL�q���\�ƂȂ�܂��B  

���ʏ��SQL

dt = db.GetDataTable($@"  
        select   
            *  
        from   
            {DbTable.T_TEST.Name}   
        where  
            {db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, 1)}�@�E�E�E�@               
    ");

���@�̏ꏊ���p�����[�^��(�e�X�g�p�ԍ�=@1)����܂��B  
�� >= <= �Ȃǂ̋L�q���\�ł��B  

���ʏ��SQL(IN��)�@�@AddWhereParameter �̏ꏊ�Ł@�e�X�g�p�ԍ� in (xx,yy) �̃f�[�^���쐬

var lst = new List<int>() { 1, 2 };  
dt = db.GetDataTable($@"  
        select   
            *  
        from   
            {DbTable.T_TEST.Name}    
        where  
            {db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, lst)}           
    ");

### DemoRun
SQLServer��DB�i�C�Ӂj���쐬  
[SQL](SQL.txt)�@�����s  

CommonData.cs�@��  
��������e���̊��ɕύX���Ă��������B  
db.ConnectionString = @"�`";  

���̌�  
SampleAndTest�v���W�F�N�g�Ł@���j���[�@�e�X�g-���s(or �f�o�b�O)-�S�Ẵe�X�g�@�Ŏ��s�\�ł��B

��SampleAndTest�v���W�F�N�g���ɐF�X�ȃT���v��������܂��B

### Settings
Visual Studio 2017�@15.3  
SQLServer2017  
��L�ŁA����m�F  
���ق��̃R���|�[�l���g��Nuget�Ŏ擾���肢���܂��B  

### Project
�f�������āA�{�C�Ŏg���Ă݂�������  
DoranekoDB�v���W�F�N�g��ProjectCommon�v���W�F�N�g�i�v�����j���K�v�ƂȂ�܂��B

�ȉ��A�e�v���W�F�N�g�̑�܂��Ȑ����ł��B  
DoranekoDB�v���W�F�N�g�@�@�@���@�f�[�^�x�[�X�A�N�Z�X(SQL�����s)�{��  
ProjectCommon�v���W�F�N�g�@ ���@�e���Ɩ��ɂ��������ʂ̃v���W�F�N�g  
SampleAndTest�v���W�F�N�g�@ ���@�e�X�g�p�v���W�F�N�g  
TemplateHelper�v���W�F�N�g�@���@DBTable�쐬�p�v���W�F�N�g

## Hints
### DBTable�N���X�̎����쐬�ɂ���
TemplateHelper�v���W�F�N�g���R���p�C����  
TemplateHelper.dll�@���쐬���ꂽ�t�H���_��  
�ȉ��̃R�}���h�����s  

C#  
dotnet TemplateHelper.dll --connectionstring "�i�ڑ�������j" --lang CS

VB.Net  
dotnet TemplateHelper.dll --connectionstring "�i�ڑ�������j" --lang VB

���s��  
CommonDataInfo.cs(vb)���쐬�����̂�  
������̃t�@�C�����ړ����ė��p���Ă��������B

## License
DoranekoDB�v���W�F�N�g  
[LGPL3.0](LICENSE.txt)

���v���W�F�N�g  
[MIT](LICENSEMIT.txt)

## Future Releases
���݁ASQLServer�iDBSQLServer.cs�j�����̑Ή��ł���  
��������������đ�DB�ipostgresql�Aoracle�j���쐬�\��
