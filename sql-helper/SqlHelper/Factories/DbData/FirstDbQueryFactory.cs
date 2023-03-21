namespace SqlHelper.Factories.DbData
{
    public class FirstDbQueryFactory : IDbQueryFactory
    {
        public string GetQueryTables() =>
            @"
                SELECT			Id = TAB.[object_id],
				                [Schema] = SCH.[name],
				                [Name] = TAB.[name]
                FROM			[sys].[schemas] SCH
                INNER JOIN		[sys].[database_principals] DPR
	                ON			DPR.[principal_id] = SCH.[principal_id]
                INNER JOIN		[sys].[tables] TAB
	                ON			TAB.[schema_id] = SCH.[schema_id]
                WHERE			DPR.[name] IN ('dbo')
                AND				SCH.[name] NOT IN ('dbo','tSQLt')
                AND				TAB.[temporal_type_desc] NOT IN ('HISTORY_TABLE');
            ";

        public string GetQueryColumns() =>
            @"
                SELECT			TableId = TAB.[object_id],
				                ColumnId = ACO.column_id,
				                [Name] = ACO.[name],
				                [Type] = TYP.[name],
				                Nullable = ACO.is_nullable,
                                IsPrimaryKey =
                                CASE
                                    WHEN EXISTS
                                    (
                                        SELECT          *
                                        FROM            [sys].[indexes] I
                                        INNER JOIN      [sys].[index_columns] ICO
                                            ON          ICO.[object_id] = I.[object_id]
                                        AND             ICO.[index_id] = I.[index_id]
                                        WHERE           I.[object_id] = TAB.[object_id]
                                        AND             I.[is_primary_key] = 1
                                        AND             ICO.[column_id] = ACO.[column_id]
                                    ) THEN 1
                                    ELSE 0
                                END
                FROM			[sys].[schemas] SCH
                INNER JOIN		[sys].[database_principals] DPR
	                ON			DPR.[principal_id] = SCH.[principal_id]
                INNER JOIN		[sys].[tables] TAB
	                ON			TAB.[schema_id] = SCH.[schema_id]
                INNER JOIN		[sys].[all_columns] ACO
	                ON			ACO.[object_id] = TAB.[object_id]
                INNER JOIN		[sys].[types] TYP
	                ON			TYP.system_type_id = ACO.system_type_id
                AND				TYP.user_type_id = ACO.user_type_id
                WHERE			DPR.[name] IN ('dbo')
                AND				SCH.[name] NOT IN ('dbo','tSQLt')
                AND				TAB.[temporal_type_desc] NOT IN ('HISTORY_TABLE');
            ";

        public string GetQueryConstraints() =>
            @"
                SELECT			Id = FKS.[object_id],
				                TargetTableId = FKC.parent_object_id,
				                SourceTableId = FKC.referenced_object_id,
				                TargetColumn = FKC.parent_column_id,
				                SourceColumn = FKC.referenced_column_id
                FROM			[sys].[schemas] SCH
                INNER JOIN		[sys].[database_principals] DPR
	                ON			DPR.[principal_id] = SCH.[principal_id]
                INNER JOIN		[sys].[tables] TAB
	                ON			TAB.[schema_id] = SCH.[schema_id]
                INNER JOIN		[sys].[foreign_keys] FKS
	                ON			FKS.[parent_object_id] = TAB.[object_id]
                INNER JOIN		[sys].[foreign_key_columns] FKC
	                ON			FKC.constraint_object_id = FKS.[object_id]
                WHERE			DPR.[name] IN ('dbo')
                AND				SCH.[name] NOT IN ('dbo','tSQLt')
                AND				TAB.[temporal_type_desc] NOT IN ('HISTORY_TABLE');
            ";
    }
}
