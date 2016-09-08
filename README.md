Authors
========

David Duran Rosich (david.duran.rosich@est.fib.upc.edu)
Johan Erik Olof Nordin (johan.erik.olof.nordin@est.fib.upc.edu)

Description
============

Penguin-Jump is an AR-game developed using Unity and Vuforia. The target
platform is an Android device with a touch input availiable. The idea of the
game origins from a AR-application where a characther moved between markers 
that were dynamically added and removed.

Our game consist of a main character, namely the penguin Tux. Tux is constantly
hungry and as soon as he can see a fish, he will start to walk towards it. But
on his way across his home and the fish, danger is present in the form of an
obstacle (trap) which he cannot come close. His only chance to avoid a 
premature death is to jump over the obstacle. Save Tux from the blades of death!!

Interaction
============

We are using three types of markers to create the game logic. The availiable
markers are an "igloo marker", a "fish marker" and a "trap marker". Each of
the markers is a frame marker associated with an unique id.

The game is initiated as soon as the "igloo marker" is detected. This marker
will spawn Tux and will trigger an idle animation.

When the "fish marker" is detected, Tux will start to move towards it. Even if
the "trap marker" is not detected yet, this will still be a fully valid state
without any obstacle present (Tux will go to grab the fish and bring it home).

Finally a "trap marker" can be added, this marker represent an obstacle (trap)
and if Tux comes too close, he will end up dying (and respawning afterwards).
However, this can be avoided if the user taps the screen. Tapping the screen
will trigger a jumping animation that can save Tux from dying if timed correcly.
