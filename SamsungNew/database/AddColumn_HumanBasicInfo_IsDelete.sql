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