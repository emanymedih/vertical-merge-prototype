# Product Principles

## Core goal

Create a clean, readable, and pleasant game loop where losing creates the feeling:

> I almost made it, and I understand how to do better next time.

The player should want to press Play Again because the game feels fair, learnable, and close to mastery.

## Core loop

External loop:

1. Receive the next cosmic body.
2. Choose a drop position.
3. Drop the body.
4. Let physics resolve.
5. Merge identical bodies.
6. Gain score.
7. Watch the container fill.
8. Save the situation or lose.
9. Start a new run.

Internal product loop:

1. See an opportunity.
2. Build a prediction.
3. Act.
4. Wait for the result.
5. Receive confirmation or correction.
6. Update the mental model.
7. Feel skill growth.
8. Get close to an achievement.
9. Lose or set a record.
10. Try a better strategy.

The internal loop matters more than feature count.

## Core emotion

Main emotion:

> I almost did it, and now I know how to do better.

Supporting emotions:

- pleasure from precise placement;
- merge anticipation;
- relief after saving the field;
- pride from creating rare cosmic bodies;
- tension as the chamber fills;
- curiosity about the next body;
- fair revenge after losing.

## Product pillars

### Simplicity

The rules should fit one sentence:

> Drop identical cosmic bodies, merge them, and do not overfill the chamber.

The player should understand control and goal within the first 10-15 seconds.

### Control

The player must feel that decisions determine the result.

Protect:

- precise input;
- clear drop point;
- predictable physics;
- visible next body;
- readable chamber boundaries;
- fair danger line;
- no sudden game over.

Physics can be lively, but not chaotic.

### Readability

The player must quickly understand:

- what body they are holding;
- what comes next;
- where matching bodies are;
- where a merge might happen;
- how full the chamber is;
- when danger is real;
- what was created after merge.

Beauty must not reduce readability.

### Anticipation

The emotional peak starts before merge.

The player should think:

> If I drop here, I might create Star.

Support this through next preview, matching-body resonance, clear field state, nearby goals, controlled randomness, and visible merge opportunities.

### Reward

Merge is the main reward.

Every merge should be clear, short, pleasant, scored, and more meaningful at high levels. High-level merges should feel like events without blocking play.

### Tension and relief

The run should alternate:

> calm -> buildup -> danger -> save -> relief -> new risk

Danger line is a core tension source, not just a loss condition. A successful merge near danger should feel like a save.

### Mastery

The player should discover strategies:

- keep large bodies low;
- avoid scattering matching bodies;
- reduce small clutter;
- preserve empty space;
- plan the next drop;
- use mass and physics;
- choose between quick merge and setup.

Each run should offer a believable way to play better than the previous one.

### Progress

Progress should be concrete.

Stronger:

> I created Neutron Star.

Weaker:

> I scored 1600 points.

Progress signals:

- score;
- best score;
- largest body in the run;
- best largest body ever;
- discovered cosmic bodies;
- next goal;
- personal records.

### Fast revenge

The path after losing must stay short:

> result -> achievement -> Play Again -> new run

Do not insert extra menus, stores, long animations, ads, or delays between loss and replay.

## Product priorities

Use this order when choosing work:

1. Fair physics.
2. Precise control.
3. Field readability.
4. Merge pleasure.
5. Run rhythm.
6. Fair game over.
7. Fast restart.
8. Visual theme.
9. Sound and haptics.
10. Additional content.

A new feature is not acceptable if it weakens one of the first seven priorities.

## Idea filter

Before implementing any new feature, ask:

1. Does it strengthen the core loop?
2. Does it make player decisions more interesting?
3. Does it improve control or readability?
4. Does it create tension and relief?
5. Does it help the player feel mastery?
6. Can it be understood without explanation?
7. Is it hiding a weak core mechanic?
8. Does it make randomness feel unfair?
9. Does it slow the restart loop?
10. Can we test it with a simple version?

If most answers are negative, do not add the feature.

## Randomness

Randomness should create variety, not decide victory or loss.

Allowed:

- variable next body;
- slightly different collisions;
- unexpected chains;
- controlled physical unpredictability.

Not allowed:

- intentionally bad drops;
- artificial forced losses;
- unexplained physics changes;
- hidden result correction;
- early unwinnable states.

The player must believe the game is fair.

## Future complications

Moving platforms, earthquakes, and other modifiers can exist later.

They should:

- change familiar strategy;
- test mastery;
- create a new rhythm;
- support the physical theme.

They should not:

- replace the base mechanic;
- appear before the player understands the rules;
- make loss feel random;
- remove control;
- be present in every run.

First, the clean version must work:

> drop -> physics -> merge -> danger -> replay

## Prototype success criteria

Do not measure prototype success by feature count.

Measure behavior:

- the player understands without long explanation;
- first merge happens quickly;
- bodies are distinguishable;
- loss reason is clear;
- Play Again is pressed without prompting;
- several runs happen in a row;
- the player changes strategy;
- the player wants the next cosmic body;
- high merge is noticed and valued.

Main test:

> Did the player press Play Again after losing?

If yes, the core is working. If not, improve the core loop, not meta-systems.

## Codex rules

Before future changes:

1. Inspect the current project structure.
2. Check whether the feature already exists.
3. Do not rewrite working systems without need.
4. Do not add large dependencies.
5. Do not expand scope independently.
6. Preserve one-screen gameplay.
7. Verify Play Mode after changes when possible.
8. Do not break physics, merge, scoring, danger line, or restart.
9. Keep tuning parameters adjustable.
10. Document changes that affect product logic.

If a technically correct solution conflicts with the product concept, the product concept wins.

## Short concept

Cosmic Merge is a short, readable, physically pleasant mobile game about creating order inside growing chaos.

The player drops cosmic bodies, merges identical objects, and tries to create rare forms without overfilling the gravitational chamber.

The main motivation is internal:

> see an opportunity, make a precise drop, create a new body, save the field, beat a record, and play one more run better.

Protect the purity of the game loop over the number of implemented features.
