# F         Q           L
# Fast      Query       Language

# Motivation

My motivation is that SQL is great, but writing SQL is a pain. Why?

1. As a developer, you normally work with the same database layouts again and again,

2. Most work will be querying test / client data in a Development / Production environment,

3. This work tends to use the same or similar queries,

4. These queries will be logically simple,

5. They make you feel like you're writing repetitive boiler-plate, even though you know how your database holds together,

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

`{table|t} SEARCH_TERM_1[ SEARCH_TERM_2[ SEARCH_TERM_3[ ... ]]]`

This returns a numbered list of tables from the layout with names matching any of the search terms.

Search terms are case-insensitive, they can match all or some of a table name. For example,

`table C` will match all these tables: **CUSTOMER**, **customer**, **LOCATION**

`table CUST` will only match: **CUSTOMER**, **customer**

`table CUST LOC` will match all the tables

When the command prompt appears again, enter a space-separated list of numbers for the tables to add to your query.

## Add filters to the output

`{filter|f} SEARCH_TERM_1[ SEARCH_TERM_2[ SEARCH_TERM_3[ ... ]]]`

This returns a numbered list of fields from the layout with names matching any of the search terms.

Search terms are case-insensitive, the can match all or some of a field name.

When the command prompt appears again, enter a space-separated list of numbers for the filters to add to your query.

## Execute the path finder

Once you are done adding tables and filters, execute the path finder.

`{execute|exec|e}`

The path finder, as the name suggests, finds all the available paths.

More on this later, but in simple terms, a path is a way of reaching all the tables required to build your query.

## Choose a path

If there is only one valid path available, fql copies your query to the clipboard and exits.

If there is more than one valid path available, you need to choose.

The first path will be printed to the console in an understandable format, followed by the command prompt.

`{next|n}` shows the next path

`{previous|p}` shows the previous path

`{current|c}` chooses the path currently being shown

There could be many valid paths to choose from. fql will show the least-complex paths first, as you are likely to choose these.

Once you have chosen a path, fql copies your query to the clipboard and exits.

## Well done!

You've used fql for the first time. You can paste your query to something like SQL Server Management Studio, fill in values for each filter, and run. Or you can use it to create a more complex query. The world is your oyster.

# Full specification

`fql [{--connection-string|-c} $CONNECTION_STRING] [{--alias|-a} $ALIAS] [{--new|-n}|{--override|-o}|{--merge|-m}] [{--print|-p}]`

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

fql is built upon constraints. Constraints are the Foreign Keys from your layout.

When you build a query, I mentioned that "all valid paths are considered".

In fql, a "valid path" can be thought of as a set of constraints that link all the tables required.

The problem is, sometimes useful constraints can be missing from your layout. Foreign Keys don't always exist between fields that represent the same things. fql by default relies on your layout for the constraints... it doesn't automatically "fill in the blanks", because this would be a nightmare. Imagine if there were constraints linking all integer fields called "Id", or all string fields called "Description".

However fql lets you add a Custom Constraint from the available list.

Custom Constraints have the same properties as Foreign Keys -

1. There is a Source / Target table,
2. There can't be an existing constraint, custom or otherwise, with the same Source / Target table,
3. The Target table contains equivalent fields spanning the Source table's Primary Key, based on field name and type.

Once fql loads and the command prompt has appeared, you can view potential Custom Constraints anytime. 

`{constraint|c}`

This returns a numbered list of available Custom Constraints.

When the command prompt appears again, enter a space-separated list of numbers for the Custom Constraints to add to your layout.

**If you are running fql with an alias**, the Custom Constraint will immediately be included in the layout stored under the alias.

**If you are running fql with just a connection string**, the Custom Constraint will be included in your layout, but will be lost once you finish.

## This is great, but what can I do with this?

Just add all the Custom Constraints to an alias that you find useful in building queries.

Even if the real database structure changes over time, you can pull these changes using the **merge** option, and still keep your Custom Constraints (if they still apply... fql silently discards the Custom Constraint forever otherwise).

# Oh and the print option

By default, fql will copy your query to the clipboard, but if you want to display it in the console window instead, supply the `{--print|-p}` option.

# WTF I didn't get any output

After building your query, you may receive this output -

`No output to generate! Your choices left no options to choose from.
There can only be one table that has no link on its primary field(s).`

This is best explained with an example.

Imagine your layout had 3 tables.

**website.CUSTOMER**  
***CustomerId***  
CustomerName

**website.ORDER**  
OrderId  
***CustomerId***  
OrderDate  
OrderCost  

**website.CUSTOMER_ADDRESS**  
CustomerAddressId  
***CustomerId***  
AddressType  
Address  

**dbo.CUSTOMER** is the Source table in 2 constraints, one with the Target table **dbo.ORDER**, one with the Target table **dbo.CUSTOMER_ADDRESS**.

What if you tried to build a query that required both **dbo.ORDER** and **dbo.CUSTOMER_ADDRESS**?

In the simple world of fql, this would produce duplicated data. The data will also be non-sensical in many contexts. fql does not like these sort of queries.

This is the reason for the "No output to generate!" message. Only **one** table is allowed to **not** be the Source table in a constraint used to build your query. In the above example, **website.ORDER** and **website.CUSTOMER_ADDRESS** satisfy this. Requiring both tables to build your query is therefore invalid.

One way to fix this is to add the field ***CustomerAddressId*** to **website.ORDER**, and a constraint linking **website.ORDER** to **website.CUSTOMER_ADDRESS**. You have probably done this already. I'm just assuming your database sucks for the purposes of this example.
