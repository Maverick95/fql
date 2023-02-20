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

Saving as file config (larger problem), specifically, viewing suggestions based on identical field names / types.
This is becoming more of a need than an ideal.


Nice to have -

What input to provide on the command line. (i.e. what connection string, but also what other options)
Better formatting for SQL output.
More efficient / neater table aliases.


I think you have four commands to start off with.
c) Reset
e) Status

Do table / filter names match the whole table or just part?

TODO : should you have handler methods return a new SqlQueryParameters instead of altering the input?
TBH probably.

BUG to look at - table alises can contain the same table twice.
I think I've got the best solution to this.
It's both the easiest solution and prevents the problem from existing.
Solution is to add a suffix to every table alias, _X, where X is the index
of the table within the solution path.
So regardless of whether a table is a duplicate or not, it has this suffix.
But this stops any alias from being used twice, without over-complicating things.

It means you have to do a bit more work in the Sql Query generator though.

Right so upon further investigation it seems this is more diffcult than you thought.
Duplicate tables exist in the Table and Filter parts of query parameters.

So for now fix this by not allowing duplicates in the path finder.

Remember, it's best to keep this dumb.

I like the idea you had before, if you pass in a list to the table alias generator,
you get back a list to zip it with.



Weird result you get, if you select Subscription, filter on ApplicationName AND VendorName,
you get no choice of path, and you should get 2?

Ahhh a subtle bug I think.


I want to rework the path finder to remove recursion.

So these are the things you need to get the product to be usable -

a) saving config, (MUST)

Path finder needs improving, paths can legitimately branch in 2 or more places.
e.g. if a table has 2 foreign keys and you filter on both the source tables.
The path finder will detect 0 paths.
This is more a problem with your narrow definition of what a path is.
It isn't just
A -> B -> C -> D
it can be
A -> B -> C
       -> D -> X

Can you detect duplicate foreign key constraints?

Okay, so the proposed solution you had for this is to consider the issue as a tree rather than a graph.

This means you need to represent the tree.

Might have to implement your own tree, which is okay, not too sure about the methods you'd need.
To be fair it will be internal, so maybe you don't need to think too much about it.

Actually a tree could be really messy.

I'm wondering if a better approach is to be dumb but transparent.

It's a diffcult problem is this.

I think a tree might make things expand and multiply in an undesirable way.

So maybe stick with a graph.

The issue is not finding A path, its finding ALL the paths.

And also not finding repeat paths.

Finding A path is not much trouble at all.

I think the way to do this is to build a matrix.

The matrix shows start node by target node.

Each required node is included.

And each entry is ALL the different ways of getting from node-to-node.

It's definitely not the end, but it's a solid beginning.

I think also a rule should be, the way of getting from node-to-node DOES NOT
include another table in the list.

It's an interesting place to start. So let's start!

Try and just build this out.
Hey maybe each entry is an IList of SqlHelperResult.
Well yeah it denotes how to get from A to B.
At this point dictate, the paths are from target to source!!

Right so for each table, you need to move up through the constraints.
If you find a source table that is required, you need to record it AND THEN MOVE ON.

For each table...

       find all constraints that have this table as target
       for each constraint...

              mark the constraint as being used
              check the table being accessed, is it a required table? (note, can be itself)
              if yes, you need to record the path in the dictionary, and then return
              this is very similar to your other path finder (??)
              hey, there might be something in this.
              it's close but it's not quite the same

Right this part works.

Now this needs to be used to get the actual results.

Theoretically what would you do?

At this point you need to loop somehow.

Hmm, what if you have loops here?
Okay, I think at this point, you have to stop looping back to the same table.

Hmm this isn't the end of this by a long way.

Okay, so after thinking about this for a couple of days I think I have a good answer.

Your helper method, to establish all the paths between required tables, is a good start.

You have to establish all paths that occur, WITHOUT USING CONSTRAINTS TWICE.

So, here is a rough method.

I think, for now, stick with a recursive method.
You can try and un-recurse things as a separate task later.

NOTE - remember the edge case where there is just 1 table.

Set up an empty list/hashtable to contain all the constraints used.
Set up an empty list/hashtable to contain all the tables found.

Pick the "next" entry in the dictionary (at first, this is the first entry, o/w)


For each entry in the dictionary,

       at this point you are "at" the target table, so flag it as being found (even if it is already found, this is key)
       then check if all tables are found, if so, you have a valid path, so add it to the results.

       for each path connecting the 2 tables,

              check if any constraints used in the path are in the constraintsUsed list
              if true, continue

              o/w, this path is valid, ensure that all constraints are added
              then repeat the method, for all dictionary entries where target table = this source table


No this is still not it.
You're not being complete.
You are still only considering paths where all tables are connected in a single link.
You need to be EXHAUSTIVE.
This means, once you have checked a path between two tables, you then need to add in addition to this, the next path.
And then the next path.
This is the tricky part.
And this will be tricky.

So I am wondering if there is still another way that is simpler.
This exhaustive approach is a possibility for sure.

No I think go with the exhaustive approach.

The exhaustive approach though is... exhaustive.
It is effectively a O(2^N) magnitude solution.

Y know in some ways it did work better as a tree.

The problem is, being exhaustive means given N "things" (paths), you need to try all combos,
e.g. 1 2 3 4
1
1 2
1 2 3
1 2 3 4
1 2   4
1   3
1   3 4
1     4
  2
  2 3
  2 3 4
  2   4
    3
    3 4
      4

15 combos = 2^4 - 1 combos

1
1 2
1 2 3
1 2 3 4
1 2 3 4 5
1 2 3   5
1 2   4
1 2   4 5
1 2     5
1   3
1   3 4
1   3 4 5
1   3   5
1     4
1     4 5
1       5
  2
  2 3
  2 3 4
  2 3 4 5
  2 3   5
  2   4
  2   4 5
  2     5
    3
    3 4
    3 4 5
    3   5
      4
      4 5
        5

31 combos = 2^5 - 1 combos

But here's the problem with the exhaustive approach,
I don't have a clear idea of what the 1,2,3,4 are.

Okay, so here's an idea.

Can you find the "root" table?

The "root" table is the end target table in the chain.
Whatever the solution is, there is one table at the "end" of the chain.
But what if a cyclical loop is found?

I think I'm really struggling with this.
I keep getting stuck in logical loops where I can't see what the actual goal is.

So maybe it is worth taking a step back.

And thinking about, what it is you are trying to achieve.

You have a graph consisting of nodes (tables) connected by one-way edges (constraints).
You have a list of required tables.
You must list every possible method of reaching all the required tables.

Then what exactly is a "method"?
A "method" is a list of constraints. The order of the constraints matters.
That's it.
Yep, that is the end goal. A list of constraints.
Two results are therefore the same if two lists match EXACTLY.
A list of constraints describe a specific journey.

Okay, so you say, "every possible method".

One issue with this problem is the concept of cyclical loops in SQL.
Using constraints, it is possible to reach the same table again.
Therefore, if you told me to include "every possible method", the list would be infinitely long.
One idea to restrict this was "each constraint can only be used once".
This would definitely stop infinite cyclical loops.

BUT, this raises another issue.
Consider the following situation.

T4
^
T1 > T2 > T3
^          v
<<<<<<<<<<<<

and T1, T3, T4 are required

All of these are valid (the order adds to the definition)

T1 to T4
T1 to T2
T2 to T3

T1 to T4
T1 to T2
T2 to T3
T3 to T1

T1 to T4
T1 to T2
T2 to T3
T3 to T1
T1 to T4

T1 to T2
T2 to T3
T3 to T1
T1 to T4

I am going to definitely say that constraints should only be used once.
This is both for my sanity and for the user.
Otherwise they would have to maybe choose every single combo and it sucks.
I am going to posit another theory.
The order of the constraints doesn't matter.
This could be magic.
If this is true, then the ideas behind computing all the combos are easier.
Because currently, you have a list of all the ways of getting from/to each required table.
The combos are just each of these switched on or off, combined with some checks.
I still like the idea of finding the "root" table though.
Because this can limit the possibilities.
At least one of the paths from the "root" table must be turned on.
I'm going to not start with this. Add this on as an optimization maybe.

Given a set of paths, you need to satisfy 2 conditions.
1) All required tables are included,
2) The constraints cover a connected span.
I feel like there is a simple rule to 2.
There's something else you're forgetting.
3) There is at least one node, that you can reach every required node starting at this node.
3) -> 2)
3) -> 1)
This is going to be a f***ing pain no matter what.
Are there any rules you can apply?
Are you going to have to brute force this?

How would you even solve this normally?
I think I have a very simple rule, given a set of constraints, for determining validity.
The set is valid if there are < 2 nodes with no parents.
I would try and prove this rule. Even if it appears to make sense.

The other major optimization I have realised is that, if there are N nodes,
there must be at least N - 1 connections.
Therefore it is not quite a O(2^N) situation.

However I think there is quite a bit of work to be done.

1) Turn the output of your helper function into a straight list, it will just be easier to handle.
2) Produce a helper function that will generate the indices you require to create subsets of constraints.
3) Implement the final result using these simple rules.
   For now, just log that you've found a result, because...
4) Change the output type (??)
  Elaborate - I think now I can only view the result as a tree structure.
  But come back to this at the end.


1 2 3
1 2 3 4
1 2 3 4 5
1 2 3   5
1 2   4
1 2   4 5
1 2     5
1   3 4


Urrrg I've forgotten which table in a constraint in the target and which is the source.
And how this relates to your algorithm.

Target - the primary key being as a reference
Source - the primary key field as stored

This means...
Target is PARENT (head of edge)
Source is CHILD (tail of edge)

This is definitely progress, BUUUT...
but but but
we have a bit of a problem
the problem is that the program is technically correct
however given a real-world example, there are 7,000+ valid combinations of joins to choose from

I think this means I am going to have to switch the way the user selects a result,
by iterating through the possibilities one-by-one until you hit the "correct" one.

And prioritize, for the better path finder, the results with less paths.
This means changing the selection algorithm, oh well.

I think this means first though I should change the output type to a tree.

Check one example first.

Results will have to be a tree, they just have to be.
This isn't a programming preference, its a fundamental property of results.

What is the tree?

A tree has a root node, this is your parent node.
Each node beneath it represents a table, should give it a table prop then.
It needs an optional set of children, each child is both the node in the tree and the path together.
Yup.

You'll have to alter a lot of things now.
But tbh, some of them you were going to alter anyway.

1) Result / path finders,
2) Path choosers and output generators.

Alter the old ones first, and put this back together so that it actually works.

There's still quite a lot of work to do here.

First main bit is the MoveToBetterPathFinder.

Aha, and I remember, this is where you had the other problem.

If you're using a better path finder, you're going to have to change the way
that the user chooses results.

Your choosing interface will have to change.

Huh.

The end result will always need to be a Route Tree, I think it's the input.

You've just told it to shove in a full list, this means you need to have all the options
computed and stored first.

What YOU want is something that effectively moves through each option one-by-one.

I'm sure I can construct a new inteface using the old.
Can we just use the foreach approach?
And settle for an ienumerable instead of a list?

PathFinder            DONE
PathChooser           DONE
OutputGenerator       DONE


TODO - if path choosers are given an empty list for selection, throw an exception
and handle this in the main.

Testing time. How are we going to test this?

I think maybe I should turn my closures into Builders.
Makes them easier to test.
Naaaah, remember, stop doing bullshit.

Another bug.
SqlQueryFactory, no tables, one field.
Ohhh I see.
If you have no tables to select, you can't even form a SQL query.
Should you default to just * then?
TODO - add tests for this.

Does it really matter about keeping the internal workings of ResultRouteTree private?
Well atm it's becoming a hindrance.

Here's a test. Make the properties public.

