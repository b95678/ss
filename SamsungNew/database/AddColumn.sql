use SS_HRM_DB
if exists(select * from syscolumns where id=object_id('Managers') and name='IsDelete') 
	print 'Managers�����д��ֶ�IsDelete'
else
begin
	print 'Managers�����޴��ֶ�IsDelete'

	--�ڱ�Managers���bit���͵���IsDelete,Ĭ��ֵΪ0
	alter table Managers add IsDelete bit default 0
	print 'Managers��ɹ�����ֶ�IsDelete'
end

if exists(select * from syscolumns where id=OBJECT_ID('AttendingInfo') and name='WorkContentId')
	print 'AttendingInfo�����д��ֶ�WorkContentId'
else
	begin
		print 'AtendingInfo �����޴��ֶ�WorkContentId'	
		--�ڱ�AtengdingInfo���int���͵���WorkContentId	
		alter table AttendingInfo add WorkContentId int 
		print 'AtengdingInfo��ɹ�����ֶ�WorkContentId'
	end

if exists(select * from syscolumns where id=OBJECT_ID('Train') and name='WorkContentId')
	print 'Train�����д��ֶ�WorkContentId'
else
	begin
		print 'Train�����޴��ֶ�WorkContentId'		
		--�ڱ�Train���int���͵���WorkContentId
		alter table Train add WorkContentId int 
		print 'Train��ɹ�����ֶ�WorkContentId'
	end

if exists(select * from syscolumns where id=OBJECT_ID('HumanBasicFile') and name='IsDelete')
	print 'HumanBasicFile�����д��ֶ�IsDelete'
else
	begin
		print 'HumanBasicFile�����޴��ֶ�IsDelete'		
		--�ڱ�HumanBasicFile���bit���͵���IsDelete
		alter table HumanBasicFile add IsDelete bit Default 0 
		print 'HumanBasicFile��ɹ�����ֶ�IsDelete'
	end
	
if exists(select * from syscolumns where id=OBJECT_ID('WorkContent') and name='IsDelete')
	print 'WorkContent�����д��ֶ�IsDelete'
else
	begin
		print 'WorkContent�����޴��ֶ�IsDelete'		
		--�ڱ�HumanBasicFile���bit���͵���IsDelete
		alter table WorkContent add IsDelete bit Default 0 
		print 'WorkContent��ɹ�����ֶ�IsDelete'
	end