use SS_HRM_DB
if exists(select * from syscolumns where id=object_id('Managers') and name='IsDelete') 
	print 'Managers表中有此字段IsDelete'
else
begin
	print 'Managers表中无此字段IsDelete'

	--在表Managers添加bit类型的列IsDelete,默认值为0
	alter table Managers add IsDelete bit default 0
	print 'Managers表成功添加字段IsDelete'
end

if exists(select * from syscolumns where id=OBJECT_ID('AttendingInfo') and name='WorkContentId')
	print 'AttendingInfo表中有此字段WorkContentId'
else
	begin
		print 'AtendingInfo 表中无此字段WorkContentId'	
		--在表AtengdingInfo添加int类型的列WorkContentId	
		alter table AttendingInfo add WorkContentId int 
		print 'AtengdingInfo表成功添加字段WorkContentId'
	end

if exists(select * from syscolumns where id=OBJECT_ID('Train') and name='WorkContentId')
	print 'Train表中有此字段WorkContentId'
else
	begin
		print 'Train表中无此字段WorkContentId'		
		--在表Train添加int类型的列WorkContentId
		alter table Train add WorkContentId int 
		print 'Train表成功添加字段WorkContentId'
	end

if exists(select * from syscolumns where id=OBJECT_ID('HumanBasicFile') and name='IsDelete')
	print 'HumanBasicFile表中有此字段IsDelete'
else
	begin
		print 'HumanBasicFile表中无此字段IsDelete'		
		--在表HumanBasicFile添加bit类型的列IsDelete
		alter table HumanBasicFile add IsDelete bit Default 0 
		print 'HumanBasicFile表成功添加字段IsDelete'
	end
	
if exists(select * from syscolumns where id=OBJECT_ID('WorkContent') and name='IsDelete')
	print 'WorkContent表中有此字段IsDelete'
else
	begin
		print 'WorkContent表中无此字段IsDelete'		
		--在表HumanBasicFile添加bit类型的列IsDelete
		alter table WorkContent add IsDelete bit Default 0 
		print 'WorkContent表成功添加字段IsDelete'
	end