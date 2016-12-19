# Ballistic kinematics
Ballistic trajectory simulation system for Unity3D.

What you can find in this repository is a generic implementation of ballistic trajectories, and the whole infrastructure to use it.
Theres an abstract Gun where you can create "any" weapon based on some parameters.


Why use this?
-------
Using raycasts is rude, you can only have a straight line between the weapon and the destination.
Using rigidbodies is slow and not very precise. A fast rigidbody can pass through thin objects. Setting rigidbodies collision detection to "continuous" has a very high computational cost.

This system allows you to simulate almost every type of projectile, from slow big bullets, arrows, etc. to the fastest minimum particle (that will lead to a simple linecast), or even particle-based weapons like flamethrowers.
This projectiles does not rely on any rigidboy/collider. They are computed in discrete time steps and are almost as expensive as simply rendering the bullet model.


Current state of the project
------
This is still a beta version. Take your own cafe is you include in any project.


ToDo
------
Object pool
Generic effect management. Avoid instantiating objects
More use examples


See also
--------
“Analytical Ballistic Trajectories with Approximately Linear Drag”, Giliam J. P. de Carpentier, International Journal of Computer Games Technology, vol. 2014, Article ID 463489, 13 pages, 2014. [pdf](http://www.decarpentier.nl/downloads/AnalyticalBallisticTrajectoriesWithApproximatelyLinearDrag-GJPdeCarpentier.pdf)
