# F         Q           L
# Fast      Query       Language

# Motivation

My motivation is that writing SQL is a massive pain.

Why? Oh the reasons.

1. As a developer, you normally work with a small number of database designs.

2. Most SQL work is querying test / client data in a Development / Production environment.

3. You tend to write the same or similar SQL queries.

4. These queries will be fairly simple and consist of SELECTs, JOINs and WHEREs

5. These queries feel like you're writing boilerplate

6. The auto-complete tools tend to get in your way

# Solution

fql builds simple queries very fast, hence the name.

It is designed around the idea that most of your queries are logically simple and are defined by -

1. Tables you want to view in the output,

2. Fields you want to filter on.

# Get Started Quickly

fql needs a **database layout** (or just **layout**) to build queries from. In variable `$CONNECTION_STRING`, store the connection string that sources your layout.

`fql --connection-string "$CONNECTION_STRING"`

Once fql loads, the command prompt will appear.

`> _`

## Add tables to the output

`table SEARCH_TERM_1[ SEARCH_TERM_2[ SEARCH_TERM_3[ ... ]]]`

This returns a numbered list of tables from the layout with names matching any of the search terms.

Search terms are case-insensitive, they can match all or some of a table name. For example,

`table C` will match all these tables: **CUSTOMER**, **customer**, **LOCATION**

`table CUST` will only match: **CUSTOMER**, **customer**

`table CUST LOC` will match all the tables

When the command prompt appears again, enter a space-separated list of numbers for the tables to add to your query.

## Add filters to the output

`filter SEARCH_TERM_1[ SEARCH_TERM_2[ SEARCH_TERM_3[ ... ]]]`

This returns a numbered list of fields from the layout with names matching any of the search terms.

Search terms are case-insensitive, the can match all or some of a field name.

When the command prompt appears again, enter a space-separated list of numbers for the filters to add to your query.

## Execute the path finder

Once you are done adding tables and filters, execute the path finder.

`execute`

The path finder, as the name suggests, finds all the available paths.

More on this later, but in simple terms, a path is a way of reaching all the tables required to build your query.

## Choose a path

If there is only one valid path available, fql copies your query to the clipboard and exits.

If there is more than one valid path available, you need to choose.

The first path will be printed to the console in an understandable format, followed by the command prompt.

`next` shows the next path

`previous` shows the previous path

`current` chooses the path currently being shown

There could be many valid paths to choose from. fql will show the least-complex paths first, as you are likely to choose these.

Once you have entered `current` to choose a path, fql copies your query to the clipboard and exits.

## Well done!

You've used fql for the first time. You can paste your query to something like SQL Server Management Studio, fill in values for each filter, and run. Or you can use it to create a more complex query. The world is your oyster.

# Full specification

`fql [{--connection-string | -c} $CONNECTION_STRING] [{--alias | -a} $ALIAS] [{--new | -n} | {--override | -o} | {--merge | -m}] [{--print | -p}]`

Here are all the ways you can run fql. I use the longer option names for descriptive purposes. Don't use these. Why do you hate yourself?

## Connection string only

`fql --connection-string "$CONNECTION_STRING"`

Retrieves a layout using connection string, but discards the layout once you are finished.

## Connection string and **new** alias

`fql --connection-string "$CONNECTION_STRING" --alias "$ALIAS" --new`

Retrieves a layout using connection string, and stores the layout under a recognizable alias for future use.

Nobody wants to type out a connection string each time fql runs. Instead, first use this form. The option `--new` indicates that you are storing a layout under a new alias.

If you use this form, and the alias already exists, fql will complain and exit.

## Alias only

`fql --alias "$ALIAS"`

Retrieves a layout from an existing alias. **You will mostly use this form.**

## Connection string and alias, override alias

`fql --connection-string "$CONNECTION_STRING" --alias "$ALIAS" --override`

Retrieves a layout using connection string, and stores the layout under an alias.

If the alias already exists, fql saves over it without any warning.

Once you run the command, the old layout is gone forever.

## Connection string and alias, merging the existing alias

`fql --connection-string "$CONNECTION_STRING" --alias "$ALIAS" --merge`

Retrieves a layout using connection string, and stores the layout under an alias, **after** attempting to merge the Custom Constraints from the existing alias.

# Custom Constraints

fql is built upon constraints

Once fql loads and the command prompt has appeared

### Oh and the print option

By default, fql will copy the resulting query to the clipboard, but if you want to output it to the console window instead,
supply the `{-p | --print}` option.
