declare @OuterClass varchar(max) = ''
declare @InnerClasses varchar(max) = ''
declare @TableName sysname = ''
declare @InnerClass varchar(max) = ''
declare @OuterClassObject varchar(max) = ''
declare @InnerClassObjectesObject varchar(max) = ''
declare @InnerClassObject varchar(max) = ''

set @OuterClassObject = 'public DB_Grout()
    {'


set @OuterClass = 'public class DB_Grout
    {'
select @OuterClass = @OuterClass + '
    public ' + TableName + ' ' + TableName + ' { get; set; }
'
from
(
    select 
        replace(T.name, ' ', '_') TableName
    FROM   sys.objects AS T
WHERE  T.type_desc = 'USER_TABLE'
) s


DECLARE @tablenameid nvarchar(max) -- or the appropriate type

DECLARE the_cursor CURSOR FAST_FORWARD
FOR SELECT T.name AS Table_Name
FROM   sys.objects AS T
WHERE  T.type_desc = 'USER_TABLE';

OPEN the_cursor
FETCH NEXT FROM the_cursor INTO @tablenameid

WHILE @@FETCH_STATUS = 0
BEGIN

set @TableName = @tablenameid
set @InnerClass = 'public class ' + @TableName + '
    {'
select @InnerClass = @InnerClass + '
    public string ' + ColumnName + ' { get; set; }
'
from
(
    select 
        replace(col.name, ' ', '_') ColumnName,
        column_id ColumnId
        
    from sys.columns col
        join sys.types typ on
            col.system_type_id = typ.system_type_id AND col.user_type_id = typ.user_type_id
    where object_id = object_id(@TableName)
) t
order by ColumnId

set @InnerClass = @InnerClass  + '
}'
set @InnerClasses=@InnerClasses+@InnerClass


set @InnerClassObject = 'this.' + @TableName + '= new '+@TableName+' { '
select @InnerClassObject = @InnerClassObject + ColumnName + '="' + ColumnName + '",'
from
(
    select 
        replace(col.name, ' ', '_') ColumnName,
        column_id ColumnId
    from sys.columns col
    where object_id = object_id(@TableName)
) t
order by ColumnId

set @InnerClassObject = @InnerClassObject  + '};'
set @InnerClassObjectesObject=@InnerClassObjectesObject+@InnerClassObject


    FETCH NEXT FROM the_cursor INTO @tablenameid
END

CLOSE the_cursor
DEALLOCATE the_cursor
set @InnerClassObjectesObject = @OuterClassObject + @InnerClassObjectesObject  + '}'
set @OuterClass=@OuterClass + @InnerClassObjectesObject +'}'+ @InnerClasses

select @OuterClass as query
