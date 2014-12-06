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