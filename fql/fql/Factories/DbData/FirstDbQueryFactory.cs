namespace SqlHelper.Factories.DbData
{
    public class FirstDbQueryFactory : IDbQueryFactory
    {
        public string GetQueryTables() =>
            @"
                -- Tables
                SELECT			Id = TAB.[object_id],
				                [Schema] = SCH.[name],
				                [Name] = TAB.[name]
                FROM			[sys].[schemas] SCH
                INNER JOIN		[sys].[tables] TAB
	                ON			TAB.[schema_id] = SCH.[schema_id]

                UNION ALL

                -- Views
                SELECT          Id = VW.[object_id],
                                [Schema] = SCH.[name],
                                [Name] = VW.[name]
                FROM            [sys].[schemas] SCH
                INNER JOIN      [sys].[views] VW
                    ON          VW.[schema_id] = SCH.[schema_id];
            ";

        public string GetQueryColumns() =>
            @"
                WITH            TABLES AS
                (
                    -- Tables
                    SELECT			Id = TAB.[object_id],
				                    [Schema] = SCH.[name],
				                    [Name] = TAB.[name]
                    FROM			[sys].[schemas] SCH
                    INNER JOIN		[sys].[tables] TAB
	                    ON			TAB.[schema_id] = SCH.[schema_id]

                    UNION ALL

                    -- Views
                    SELECT          Id = VW.[object_id],
                                    [Schema] = SCH.[name],
                                    [Name] = VW.[name]
                    FROM            [sys].[schemas] SCH
                    INNER JOIN      [sys].[views] VW
                        ON          VW.[schema_id] = SCH.[schema_id]
                )
                SELECT			TableId = TABLES.Id,
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
                                        WHERE           I.[object_id] = TABLES.Id
                                        AND             I.[is_primary_key] = 1
                                        AND             ICO.[column_id] = ACO.[column_id]
                                    ) THEN 1
                                    ELSE 0
                                END
                FROM			TABLES
                INNER JOIN		[sys].[all_columns] ACO
	                ON			ACO.[object_id] = TABLES.Id
                INNER JOIN		[sys].[types] TYP
	                ON			TYP.system_type_id = ACO.system_type_id
                AND				TYP.user_type_id = ACO.user_type_id;
            ";

        public string GetQueryConstraints() =>
            @"
                WITH            TABLES AS
                (
                    -- Tables
                    SELECT			Id = TAB.[object_id],
				                    [Schema] = SCH.[name],
				                    [Name] = TAB.[name]
                    FROM			[sys].[schemas] SCH
                    INNER JOIN		[sys].[tables] TAB
	                    ON			TAB.[schema_id] = SCH.[schema_id]

                    UNION ALL

                    -- Views
                    SELECT          Id = VW.[object_id],
                                    [Schema] = SCH.[name],
                                    [Name] = VW.[name]
                    FROM            [sys].[schemas] SCH
                    INNER JOIN      [sys].[views] VW
                        ON          VW.[schema_id] = SCH.[schema_id]
                )
                SELECT			Id = FKS.[object_id],
				                TargetTableId = FKC.parent_object_id,
				                SourceTableId = FKC.referenced_object_id,
				                TargetColumn = FKC.parent_column_id,
				                SourceColumn = FKC.referenced_column_id
                FROM			TABLES
                INNER JOIN		[sys].[foreign_keys] FKS
	                ON			FKS.[parent_object_id] = TABLES.Id
                INNER JOIN		[sys].[foreign_key_columns] FKC
	                ON			FKC.constraint_object_id = FKS.[object_id];
            ";
    }
}
