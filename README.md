# F         Q           L
# Fast      Query       Language

## Motivation

My motivation is that writing SQL is a massive pain.

Why? Oh the reasons.

1. As a developer, you normally work with a small number of database designs.

2. Most SQL work is querying test / client data in a Development / Production environment.

3. You tend to write the same or similar SQL queries.

4. These queries will be fairly simple and consist of SELECTs, JOINs and WHEREs

5. These queries feel like you're writing boilerplate

## Solution

fql outputs simple SQL queries very quickly, you can then modify these as required.

It operates on the idea that most of the work in writing a SQL query involves specifying -

1. The tables you want to view in the output,

2. The fields you want to filter on.

## Command-line options

`fql [{-c | --connection-string} CONNECTION_STRING] [{-a | --alias} ALIAS] [{--new | -n} | {--override | -o} | {--merge | -m}] [{--print | -p}]`

### Connecting string only

`fql -c "my_connection_string"`

Retrieves a database layout using connecting string **my_connecting_string**.

### Alias only

`fql -a my_alias`

Retrieves a database layout stored under alias **my_alias**.

You will mostly use this form. 

### Connection string and alias

You must provide one of these options: --new, --override, --merge

This is to avoid unwanted outcomes by giving fql a very specific instruction.

`fql -c "my_connection_string" -a my_alias --new`

Retrieves a database layout using **my_connection_string**, and attempts to store the layout under alias **my_alias**.

If **my_alias** already exists, fql complains and exits.

Once the layout is stored, you can run fql using the alias-only version.

`fql -c "my_connection_string" -a my_alias --override`

Retrieves a database layout using **my_connection_string**, and stores the layout under alias **my_alias**.

This overrides any layout previously stored under **my_alias**.

There is no warning given. Once you run the command, the old layout is gone forever.

`fql -c "my_connection_string" -a my_database --merge`

Retrieves a database layout using **my_connection_string**, and saves it for later under the alias **my_alias**,
after attempting to merge the Custom Constraints already stored in **my_alias**.

### Oh and the print option

By default, fql will copy the resulting SQL query to the clipboard, but if you want to output it to the console window instead,
supply the `{-p | --print}` option.

## fql Interface

Once fql loads, the command prompt will appear

`> _`

### Add tables to the output

`{t|table} SEARCH_TERM_1[ SEARCH_TERM_2[ SEARCH_TERM_3[ ... ]]]`

`t Orders Customers`

`table Orders Customers`

Returns a numbered list of layout tables with names matching any of the search terms.
Search terms are case-insensitive and can match all or some of a table name.  

e.g.

`table C` will match all these tables: **CUSTOMER**, **customer**, **LOCATION**



