window.bob = {

    animator: null,

    init: function (id) {

        console.log("bob.init", id);

        const element = document.getElementById(id);

        if (!element) {
            console.log("Bob element missing:", id);
            return;
        }

        window.bob.animator =
            new BobAminator(element);

        window.bob.play('Rock');
        window.bob.play('PanicLeft');
        window.bob.play('GlitchRight');
        window.bob.play('Lift');
        window.bob.play('Fall');
        window.bob.play('FloorIt');
        window.bob.play('PanicLeft');
        window.bob.play('PanicLeft');
        window.bob.play('JumpEscalation');
        window.bob.play('NavigatorClimb');

        console.log("Bob ready");
    },


    play: function (name) {

        if (!window.bob.animator) {

            console.log(
                "Bob not ready yet"
            );

            return;
        }

        window.bob.animator.play(name);
    },


    // THE CHAOS ENGINE: One call, random sequential rampage
    playChaosRoutine: function (movesCount = 500) {
        if (!window.bob.animator) return;

        const b = window.bob.animator;

        // Define a pool of functions we can safely chain
        const actionPool = [
            () => b.NavigatorClimb(),
            // () => b.playEverestNavigatorClimb(),
            // () => b.playFujiNavigatorClimb(),
            () => b.floorIt(),
            () => b.hop(20, 200),
            () => b.rock(15, 100, 3),
            () => {
                b.move(-150, 0, 400);
                b.move(150, 0, 400);
            },
            () => {
                b.rotate(90, 400);
                b.rotate(-90, 400);
            },
            () => {
                b.panicRoll(-300, 1000);
                b.panicRoll(200, 1000);
            },
            () => b.heavyLift(),
            () => b.radarSweep(),
            () => {
                b.teleportGlitch(-100, -50);
                b.teleportGlitch(100, 50);
            }
        ];

        console.log(`Bob is planning ${movesCount} random acts of chaos...`);

        for (let i = 0; i < movesCount; i++) {
            const randomIndex = Math.floor(Math.random() * actionPool.length);
            // Execute the array item to push it into Bob's internal Promise queue
            actionPool[randomIndex]();
        }

        // Always end the random sequence by ensuring he returns to baseline bounds safely
        b.cliffFall();
    }
};