# Design Notes

## Core Priority

The first milestone is a reliable and satisfying base loop:

- drag and drop balls
- 2D physics settle naturally
- same-level balls merge reliably
- score and game over are clear
- restart works

Do not add progression mechanics until the base physics feels good.

## Future Difficulty Mechanics

### Moving Platforms

At later difficulty stages, moving platforms can appear inside the container.

Design intent:

- disrupt the player's intended drop path
- create temporary landing surfaces
- force the player to think about timing, not only horizontal aim
- make balls settle in less predictable areas without feeling unfair

Early prototype constraints:

- platforms should be simple static or kinematic 2D colliders
- movement should be slow and readable
- platforms should not completely block the container
- avoid adding this before the base merge loop is tuned

Open questions:

- Should platforms be permanent for a run or appear only during specific phases?
- Should platforms move horizontally, vertically, rotate, or combine patterns?
- Can balls merge while resting on platforms, or should platforms mainly act as temporary obstacles?

### Earthquake Shocks

At higher levels, short earthquake events can shake the container for 2-3 seconds.

Design intent:

- heavy balls should settle downward more strongly
- light balls should remain more likely to shift, bounce, or stay near the top
- the event should create controlled chaos without feeling random

Early prototype constraints:

- use short impulses or temporary gravity/force modifiers
- scale the effect by ball mass
- keep the visual camera shake separate from the actual physics force
- do not trigger during the first few seconds of a run
- avoid triggering too often, because it can undermine player agency

Open questions:

- Should earthquakes be timed by score, ball level, or overfill pressure?
- Should the player get a warning before a shock starts?
- Should shocks help the player recover from overfill, or mostly increase difficulty?
