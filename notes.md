Writing SQL queries is repetitive and time-consuming.
Also, the SQL code auto-gen is next to useless.
The time spent adapting to use it efficiently is a task in itself.

The reality is, most SQL queries you write are similar.
They contain joins between tables based on equality comparisons,
and WHERE filters based on equality comparisons.

The aim behind this project is, there must be a simpler way of quickly generating
boiler-plate code for these queries.

A good way to look at this problem is to identify all the links between tables
( DIMs, FACTs, etc. )

Your theory is, all the various links between tables mean all data is reduced to
a single view, the result of the join between all tables with constraints.
The WHERE queries and the fields included in the view in reality
restrict the joins required.

The difficulty will be in how to decide what links are valid and included.

Because SQL relationships can get complicated fast.

Also, it may be the case that defining relationships requires subjectivity.
Based on field names / types alone, or just constraints, may not be enough to
add context. Or it may be confusing.
For example, if there are 2 separate paths of getting from Table A to Table B, which is valid?
Part of this might have to involve you specifying the path.
This makes sense.

As a thin slice, may be worth starting with just constraints.

How can we find out about foreign key constraints from the database?

What is a SQL principal?

https://learn.microsoft.com/en-us/sql/relational-databases/security/authentication-access/principals-database-engine?view=sql-server-ver16

https://learn.microsoft.com/en-us/sql/relational-databases/system-catalog-views/sys-database-principals-transact-sql?view=sql-server-ver16

Which schemas do we care about?

Owned by database principal dbo?

I also think you should exclude tSQLt schema
Maybe you should have the option to include tSQLt if you wish, but exclude by default.
I would also specify to include the dbo schema if you wish.
I would also exclude history tables by default.

These are your tables. Only one piece of data.

The next are the fields, and the keys.

https://learn.microsoft.com/en-us/sql/relational-databases/tables/create-foreign-key-relationships?view=sql-server-ver16

Note with foreign keys, there are column-level and table-level keys.

https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints?view=sql-server-ver16

What's the next step?
We effectively have all the data we need.

Well we need to extract it in the right format. We have -

tables
fields
constraints / constraint details

Tables are nodes, constraints are edges
Fields will be used to identify the tables used to generate the paths required.

Code-wise, first step I think is to define an interface factory that returns... something.

I think you need an object that contains ALL the data required, all the data required to run the algorithm.

Don't take more than you need.

Done with first part.

Remember that the data returned is just that, data, it's not a graph.
Or is it? Do you have all the data here to run a proper model?

I think I need to sit and think for a bit about the next part.

Its at this point I think that you can specify -

a) the TABLEs you are interested in VIEWing fields from,
b) the FIELDs you are interested in QUERYing on,

and use these to build the query up.

I'm not sure yet how you'll get these inputs from the user.
So begin at the end... the TABLEs are a list of Ids, the FIELDs are a list of TABLEs / INDEXes.

You can combine both these into a list of required tables.

First assumption that must be checked - all supplied tables are reachable from a single query.
o/w this is useless.

A) WLOG, edges travel from TARGET tables to SOURCE tables.
B) Paths must begin/end at a table in the given list.
C) Cycles are not allowed.

The dumb option needs to be... dumb.

1) Any table in the list could be the start node.
2) I don't know which will be the end node.
3) I don't know how many valid paths there will be (could be 0).

This strikes me initially as a very stupid depth-first search.
I think we can relax assumption C) to say that constraints can only be used once.

What next?

Required -

Choosing the path instead of just taking the first available.

Saving as file config (larger problem), specifically, viewing suggestions based on identical field names / types.
This is becoming more of a need than an ideal.


Nice to have -

What input to provide on the command line. (i.e. what connection string, but also what other options)
Better formatting for SQL output.
More efficient / neater table aliases.



How to build up the tables / views.
The whole aim of this is to reduce input from the user.

I think you have four commands to start off with.
a) Add table DONE
b) Add filter DONE
c) Reset
d) Execute DONE
e) Status

Do table / filter names match the whole table or just part?

TODO : should you have handler methods return a new SqlQueryParameters instead of altering the input?
TBH probably.

Add some new lines to the output.
