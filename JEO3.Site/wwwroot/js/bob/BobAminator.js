window.BobAminator = class {

    constructor(element) {
        this.el = element;

        if (!this.el) {
            throw new Error("Bob has no element");
        }

        // State tracking
        this.x = 0;
        this.y = 0;
        this.rotation = 0;
        this.scaleX = 1; // Used to flip left/right naturally

        // Core sequential queue pipeline
        this.queue = Promise.resolve();

        // Initial render anchor
        this.render();

        this.initRandomCostumes(3000); // 15000ms = 15 seconds
    }

    /**
     * Updates the element's actual CSS transform properties 
     * combining current translation, rotation, and mirror scaling.
     */
    render() {
        this.el.style.transform = `
            translate3d(${this.x}px, ${this.y}px, 0)
            rotate(${this.rotation}deg)
            scaleX(${this.scaleX})
        `;
    }

    /**
     * Enqueues an execution block to ensure animations never overlap.
     */
    enqueue(action) {
        this.queue = this.queue.then(async () => {
            try {
                await action();
            } catch (e) {
                console.log("Bob animation interrupted:", e);
            }
            this.render(); // Lock in final state coordinates
        });
        return this;
    }

    wait(ms) {
        return this.enqueue(() => new Promise(resolve => setTimeout(resolve, ms)));
    }

    /**
     * Precise relative hop without breaking translation/rotation state.
     */
    hop(height = 10, duration = 250) {
        return this.enqueue(() => {
            // console.log("Bob plays hop");
            return this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` },
                { transform: `translate3d(${this.x}px, ${this.y - height}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})`, easing: 'ease-out' },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})`, easing: 'ease-in' }
            ], {
                duration,
                fill: "forwards"
            }).finished;
        });
    }

    /**
     * Rotates back and forth sequentially relative to his current rotation.
     */
    rock(degrees = 10, duration = 250, iterations = 3) {
        return this.enqueue(async () => {
            for (let i = 0; i < iterations; i++) {
                // console.log("Bob plays rock");

                await this.el.animate([
                    { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` },
                    { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation - degrees}deg) scaleX(${this.scaleX})` },
                    { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation + degrees}deg) scaleX(${this.scaleX})` },
                    { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` }
                ], {
                    duration: duration * 2,
                    easing: "ease-in-out"
                }).finished;
            }
        });
    }

    /**
     * Standard translation vector updates state safely upon completion.
     */
    move(dx, dy, duration = 500, easing = "ease-in-out") {
        return this.enqueue(() => {
            // console.log("Bob plays move");
            // Adjust body facing direction based on heading
            if (dx !== 0) {
                this.scaleX = dx < 0 ? -1 : 1;
            }

            return this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` },
                { transform: `translate3d(${this.x + dx}px, ${this.y + dy}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` }
            ], {
                duration,
                easing,
                fill: "forwards"
            }).finished.then(() => {
                this.x += dx;
                this.y += dy;
            });
        });
    }

    /**
     * Rotates safely while preserving structural absolute positions.
     */
    rotate(degrees, duration = 500) {
        return this.enqueue(() => {
            // console.log("Bob plays rotate");
            const targetRotation = this.rotation + degrees;
            return this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${targetRotation}deg) scaleX(${this.scaleX})` }
            ], {
                duration,
                easing: "ease-in-out",
                fill: "forwards"
            }).finished.then(() => {
                this.rotation = targetRotation;
            });
        });
    }

    /**
     * ROUTINE 1: Escalating jumps leading into a zero-gravity drifting arc.
     */
    playJumpEscalation() {
        // Jump, jump a little higher, jump a little higher
        this.hop(15, 300);
        this.hop(35, 400);
        this.hop(65, 500);

        // ...then jump too high and float!
        this.enqueue(async () => {
            // console.log("Bob plays jump");
            const frames = 60;
            const duration = 1500;
            const floatHeight = 250;
            const horizontalDrift = -150; // drift left out into space

            const keyframesX = [];
            const keyframesY = [];

            // Compute physics arc math natively inside the animation container
            for (let i = 0; i <= frames; i++) {
                const t = i / frames; // 0 to 1

                // Sine wave oscillation for x positioning (increasing-then-decreasing variant)
                const currentX = this.x + (horizontalDrift * t) + (Math.sin(t * Math.PI * 2) * 25);
                // Parabolic arc for apex height jump physics
                const currentY = this.y - (Math.sin(t * Math.PI) * floatHeight);

                keyframesX.push({
                    transform: `translate3d(${currentX}px, ${currentY}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})`
                });
            }

            await this.el.animate(keyframesX, {
                duration,
                easing: "linear",
                fill: "forwards"
            }).finished;

            // Apply mutation state adjustments upon land
            this.x += horizontalDrift;
        });

        return this;
    }

    /**
     * ROUTINE 2: Flings Bob left or right until he collides flat against the view boundaries.
     */
    playBoundaryBounce(direction = "left") {
        this.enqueue(async () => {
            // console.log("Bob plays playBoundaryBounce");
            const rect = this.el.getBoundingClientRect();
            // Determine distance to viewport bounds
            const distanceToBounds = direction === "left"
                ? -(window.innerWidth - rect.right - 20) // Account for CSS initial positioning
                : 20;

            const jumpHeight = 120;
            const duration = 800;
            const impactRotation = direction === "left" ? -90 : 90;

            // Arc towards boundary while rotating 90 degrees mid-flight
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` },
                { transform: `translate3d(${this.x + (distanceToBounds / 2)}px, ${this.y - jumpHeight}px, 0) rotate(${this.rotation + (impactRotation / 2)}deg) scaleX(${this.scaleX})`, easing: "ease-out" },
                { transform: `translate3d(${this.x + distanceToBounds}px, ${this.y - (jumpHeight / 4)}px, 0) rotate(${this.rotation + impactRotation}deg) scaleX(${this.scaleX})`, easing: "ease-in" }
            ], {
                duration,
                fill: "forwards"
            }).finished;

            this.x += distanceToBounds;
            this.rotation += impactRotation;

            // Sliding or scaling down back to the bottom baseline coordinate safely
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` }
            ], { duration: 400, fill: "forwards" }).finished;
        });

        return this;
    }

    /**
     * ROUTINE 3: The precise Navigator Widget structural climbing route.
     * Looks for a selector (e.g., matching a Radzen sidebar panel anchor) or defaults to relative bounds.
     */
    playNavigatorClimb(selector = '.rz-sidebar, #navigator-widget') {
        const widget = document.querySelector(selector);

        // Base calibrations (Fallbacks used if DOM target element isn't found)
        let climbWidth = 250;
        let climbHeight = 400;

        if (widget) {
            const wRect = widget.getBoundingClientRect();
            const bRect = this.el.getBoundingClientRect();
            climbWidth = Math.abs(bRect.left - wRect.left);
            climbHeight = Math.abs(bRect.bottom - wRect.top);
        }

        // Hop 5x and back down (3 times total)
        this.hop(5, 150);
        this.hop(5, 150);
        this.hop(5, 150);

        // Rock back and forth 10 degrees counter & clockwise 3 times
        this.rock(10, 120, 3);
        this.wait(200);

        // Hop 2 more times
        this.hop(10, 200);
        this.hop(10, 200);

        // Move left 100px for a running start
        this.move(-100, 0, 400);

        // Scoot along x axis until bottom left x/y of navigator widget
        this.move(-(climbWidth - 100), 0, 700);

        // Move vertically up along y to the top left coordinate of the widget
        this.move(0, -climbHeight, 900);

        // Move along x again (50px or middle of navigator element width)
        this.move(50, 0, 350);

        // Hop 5 times on top of it
        this.hop(15, 180);
        this.hop(15, 180);
        this.hop(15, 180);
        this.hop(15, 180);

        // Move back -50px along y back to navigator top-left x/y context
        // (Assuming you meant back along the X path, or Y adjustment—handled as returning to top-left edge here)
        this.move(-50, 0, 350);

        // Rotate -30 degrees counterclockwise
        this.rotate(-30, 300);

        // Slowly rotate 360 degrees 3 times while absolute x/y coordinates slowly increment/decrement 
        // down toward the parent container bottom origin x/y position.
        this.enqueue(async () => {
            const duration = 2000;
            const targetX = 0; // Returning to stylesheet fixed bottom right anchors
            const targetY = 0;

            await this.el.animate([
                {
                    transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})`
                },
                {
                    transform: `translate3d(${targetX}px, ${targetY}px, 0) rotate(${this.rotation + (360 * 7)}deg) scaleX(${this.scaleX})`,
                    easing: "ease-in-out"
                }
            ], {
                duration,
                fill: "forwards"
            }).finished;

            // Reset coordinate tracking cleanly at home base
            this.x = targetX;
            this.y = targetY;
            this.rotation = 0;
            this.scaleX = 1;
        });

        return this;
    }

    async playEverestNavigatorClimb() {
        return this.enqueue(async () => {
            // console.log("Bob plays playEverestNavigatorClimb");
            // Locate the .navigator SVG container
            const navSvg = document.querySelector('svg.navigator');
            if (!navSvg) {
                // console.log("Could not find svg.navigator for Bob to climb!");
                return;
            }

            // Get live coordinates for the layout
            const navRect = navSvg.getBoundingClientRect();
            const parentRect = this.el.parentElement.getBoundingClientRect();

            // console.log(navRect);

            // Calculate anchor positions relative to Bob's offset parent
            const navLeftX = navRect.left - parentRect.left;
            const navTopY = navRect.top - parentRect.top;
            const navWidth = navRect.width;

            // Save original styles to restore them when done
            const originalZIndex = this.el.style.zIndex;
            const originalTransformOrigin = this.el.style.transformOrigin;

            // Pivot from his bottom-center so rotations and scales make physical sense
            this.el.style.transformOrigin = "bottom center";

            // --- PHASE 1: PEER FROM BEHIND THE LEFT SIDE ---
            // Drop z-index to duck behind the navigator
            this.el.style.zIndex = "-1";

            // Sneak over to the left edge minus 7px, and tilt -45 deg to "peer out"
            this.x = navLeftX - 7;
            this.y = navTopY + (navRect.height * 0.5); // Center vertically against the nav side

            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(-45deg)` }
            ], { duration: 800, fill: "forwards", easing: "ease-out" }).finished;

            // Brief pause to look around
            await new Promise(r => setTimeout(r, 400));

            // --- PHASE 2: THE WIND-UP BACKWARD ---
            // Wind back x-50px (pulling back to run/jump)
            this.x -= 50;
            await this.el.animate([
                { transform: `translate3d(${this.x + 50}px, ${this.y}px, 0) rotate(-45deg)` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(-15deg)`, easing: "ease-in-out" }
            ], { duration: 400, fill: "forwards" }).finished;

            // --- PHASE 3: THE CHARGE & CLIMB UP THE WALL ---
            // Charge back to the edge, drop rotation, pop z-index back up to climb over it
            this.x = navLeftX;
            this.y = navTopY; // Target the top edge of the navigator
            this.el.style.zIndex = "10";

            await this.el.animate([
                { transform: `translate3d(${this.x - 50}px, ${navRect.top - parentRect.top + (navRect.height * 0.5)}px, 0) rotate(-15deg)` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(0deg) scale(0.9, 1.1)`, easing: "ease-in" }
            ], { duration: 600, fill: "forwards" }).finished;

            // --- PHASE 4: THE CELEBRATORY CHOPS ---
            // Shimmy to the center top of the widget
            this.x = navLeftX + (navWidth / 2) - (this.el.getBoundingClientRect().width / 2);
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0)` }
            ], { duration: 300, fill: "forwards" }).finished;

            // Hop a handful of times (3 high-energy bounces)
            for (let i = 0; i < 3; i++) {
                await this.el.animate([
                    { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1.1, 0.9)` }, // squash
                    { transform: `translate3d(${this.x}px, ${this.y - 25}px, 0) scale(0.9, 1.1)`, easing: "ease-out" }, // airborne apex
                    { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1, 1)`, easing: "ease-in" } // land
                ], { duration: 400 }).finished;
            }

            // Clean up temporary styles back to baseline
            this.el.style.zIndex = originalZIndex;
            this.el.style.transformOrigin = originalTransformOrigin;
        });
    }

    async playEverestNavigatorClimb2() {
        return this.enqueue(async () => {
            // console.log("Bob plays playEverestNavigatorClimb2");
            // Locate the .navigator SVG container
            const navSvg = document.querySelector('svg.navigator');
            if (!navSvg) {
                // console.log("Could not find svg.navigator for Bob to climb!");
                return;
            }

            // Get live coordinates for the layout
            const navRect = navSvg.getBoundingClientRect();
            const parentRect = this.el.parentElement.getBoundingClientRect();
            // console.log(navRect);
            // Calculate anchor positions relative to Bob's offset parent
            const navLeftX = navRect.left - parentRect.left;
            const navTopY = navRect.top - parentRect.top;
            const navWidth = navRect.width;

            // Save original styles to restore them when done
            const originalZIndex = this.el.style.zIndex;
            const originalTransformOrigin = this.el.style.transformOrigin;

            // Pivot from his bottom-center so rotations and scales make physical sense
            this.el.style.transformOrigin = "bottom center";

            // --- PHASE 1: PEER FROM BEHIND THE LEFT SIDE ---
            // Drop z-index to duck behind the navigator
            this.el.style.zIndex = "-1";

            // Sneak over to the left edge minus 7px, and tilt -45 deg to "peer out"
            this.x = navLeftX - 7;
            this.y = navTopY + (navRect.height * 0.5); // Center vertically against the nav side

            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(-45deg)` }
            ], { duration: 800, fill: "forwards", easing: "ease-out" }).finished;

            // Brief pause to look around
            await new Promise(r => setTimeout(r, 400));

            // --- PHASE 2: THE WIND-UP BACKWARD ---
            // Wind back x-50px (pulling back to run/jump)
            this.x -= 50;
            await this.el.animate([
                { transform: `translate3d(${this.x + 50}px, ${this.y}px, 0) rotate(-45deg)` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(-15deg)`, easing: "ease-in-out" }
            ], { duration: 400, fill: "forwards" }).finished;

            // --- PHASE 3: THE CHARGE & CLIMB UP THE WALL ---
            // Charge back to the edge, drop rotation, pop z-index back up to climb over it
            this.x = navLeftX;
            this.y = navTopY; // Target the top edge of the navigator
            this.el.style.zIndex = "10";

            await this.el.animate([
                { transform: `translate3d(${this.x - 50}px, ${navRect.top - parentRect.top + (navRect.height * 0.5)}px, 0) rotate(-15deg)` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(0deg) scale(0.9, 1.1)`, easing: "ease-in" }
            ], { duration: 600, fill: "forwards" }).finished;

            // --- PHASE 4: THE CELEBRATORY CHOPS ---
            // Shimmy to the center top of the widget
            this.x = navLeftX + (navWidth / 2) - (this.el.getBoundingClientRect().width / 2);
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0)` }
            ], { duration: 300, fill: "forwards" }).finished;

            // Hop a handful of times (3 high-energy bounces)
            for (let i = 0; i < 3; i++) {
                await this.el.animate([
                    { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1.1, 0.9)` }, // squash
                    { transform: `translate3d(${this.x}px, ${this.y - 25}px, 0) scale(0.9, 1.1)`, easing: "ease-out" }, // airborne apex
                    { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1, 1)`, easing: "ease-in" } // land
                ], { duration: 400 }).finished;
            }

            // Clean up temporary styles back to baseline
            this.el.style.zIndex = originalZIndex;
            this.el.style.transformOrigin = originalTransformOrigin;
        });
    }

    async playFujiNavigatorClimb() {
        return this.enqueue(async () => {
            // console.log("Bob plays playFujiNavigatorClimb");
            // Locate the element (Tries the SVG, falls back to its layout container if needed)
            let targetEl = document.querySelector('svg.navigator');

            // Fallback: If the SVG itself is hidden or unstyled, look for the parent container/grid wrapper
            if (!targetEl) {
                targetEl = document.querySelector('.navigator') || document.querySelector('[class*="navigator"]');
            }

            if (!targetEl) {
                // console.log("Could not find any navigation target for Bob!");
                return;
            }

            // Get live positions
            const navRect = targetEl.getBoundingClientRect();
            const parentRect = this.el.parentElement.getBoundingClientRect();

            // CRITICAL FIX: If dimensions are 0, the element isn't rendered or visible yet
            if (navRect.width === 0 && navRect.left === 0) {
                // console.log("Target found, but its layout bounds are 0. Is it hidden or in a closed tab?");
                return;
            }

            // Calculate anchor positions relative strictly to Bob's parent element bounds
            const navLeftX = navRect.left - parentRect.left;
            const navTopY = navRect.top - parentRect.top;
            const navWidth = navRect.width;
            const navHeight = navRect.height;

            // Save original styling benchmarks
            const originalZIndex = this.el.style.zIndex;
            const originalTransformOrigin = this.el.style.transformOrigin;
            this.el.style.transformOrigin = "bottom center";

            // --- PHASE 1: PEER FROM BEHIND THE LEFT SIDE ---
            this.el.style.zIndex = "-1";

            // Lock onto the left edge, tucked slightly behind (-7px)
            this.x = navLeftX - 7;
            this.y = navTopY + (navHeight * 0.5);

            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(-45deg)` }
            ], { duration: 800, fill: "forwards", easing: "ease-out" }).finished;

            await new Promise(r => setTimeout(r, 400));

            // --- PHASE 2: THE WIND-UP BACKWARD ---
            this.x -= 50;
            await this.el.animate([
                { transform: `translate3d(${this.x + 50}px, ${this.y}px, 0) rotate(-45deg)` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(-15deg)`, easing: "ease-in-out" }
            ], { duration: 400, fill: "forwards" }).finished;

            // --- PHASE 3: THE CHARGE & CLIMB UP THE WALL ---
            this.x = navLeftX;
            this.y = navTopY;
            this.el.style.zIndex = "10";

            await this.el.animate([
                { transform: `translate3d(${this.x - 50}px, ${navTopY + (navHeight * 0.5)}px, 0) rotate(-15deg)` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(0deg) scale(0.9, 1.1)`, easing: "ease-in" }
            ], { duration: 600, fill: "forwards" }).finished;

            // --- PHASE 4: THE CELEBRATORY HOPS ---
            // Center Bob safely on top of the layout element
            const bobWidth = this.el.getBoundingClientRect().width || 30;
            this.x = navLeftX + (navWidth / 2) - (bobWidth / 2);

            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0)` }
            ], { duration: 300, fill: "forwards" }).finished;

            // Hop a handful of times
            for (let i = 0; i < 3; i++) {
                await this.el.animate([
                    { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1.1, 0.9)` },
                    { transform: `translate3d(${this.x}px, ${this.y - 25}px, 0) scale(0.9, 1.1)`, easing: "ease-out" },
                    { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1, 1)`, easing: "ease-in" }
                ], { duration: 400 }).finished;
            }

            // Restore baseline
            this.el.style.zIndex = originalZIndex;
            this.el.style.transformOrigin = originalTransformOrigin;
        });
    }

    //1. The Database Corruption Meltdown (panicRoll)

    //The Vibe: You just triggered a cyclical dependency or a query timed out.Bob completely loses his mind, vibrating rapidly before rolling across the screen like a bowling ball.
    panicRoll(distance = -200, duration = 1000) {
        return this.enqueue(async () => {
            // console.log("Bob plays panicRoll");
            // Step 1: Rapid panicking vibration
            const shakeIntensity = 4;
            for (let i = 0; i < 5; i++) {
                await this.el.animate([
                    { transform: `translate3d(${this.x - shakeIntensity}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` },
                    { transform: `translate3d(${this.x + shakeIntensity}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` }
                ], { duration: 50 }).finished;
            }

            // Step 2: Roll away at high speed
            const spins = distance < 0 ? -2 : 2;
            if (distance !== 0) this.scaleX = distance < 0 ? -1 : 1;

            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` },
                {
                    transform: `translate3d(${this.x + distance}px, ${this.y}px, 0) rotate(${this.rotation + (360 * spins)}deg) scaleX(${this.scaleX})`,
                    easing: "ease-in"
                }
            ], { duration }).finished;

            this.x += distance;
            this.rotation += (360 * spins);
        });
    }


    // 2. The Heavy SQL Query Lift (heavyLift)

    // The Vibe: Simulated physical exertion. Bob tries to lift an imaginary heavy object(like a massive metadata payload), struggles, squishes down under the weight, and violently pops it up over his head.
    heavyLift(duration = 2000) {
        return this.enqueue(async () => {
            // console.log("Bob plays heavyLift");
            // Squish down under the "weight"
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1, 1)` },
                { transform: `translate3d(${this.x}px, ${this.y + 8}px, 0) scale(1.3, 0.6)', easing: "ease-out"` }
            ], { duration: duration * 0.3, fill: "forwards" }).finished;

            // Tremble under pressure
            for (let i = 0; i < 3; i++) {
                await this.el.animate([
                    { transform: `translate3d(${this.x - 2}px, ${this.y + 8}px, 0) scale(1.3, 0.6)` },
                    { transform: `translate3d(${this.x + 2}px, ${this.y + 8}px, 0) scale(1.3, 0.6)` }
                ], { duration: 100 }).finished;
            }

            // Explode upward (The Triumph!)
            await this.el.animate([
                {
                    transform: `translate3d(${this.x}px, ${this.y + 8}px, 0) scale(1.3, 0.6)`
                },
                { transform: `translate3d(${this.x}px, ${this.y - 40}px, 0) scale(0.8, 1.4), easing: "ease-out"` },
                {
                    transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1, 1), easing: "ease-in"`
                }
            ], { duration: duration * 0.5, fill: "forwards" }).finished;
        });
    }

    // The Glitch in the Matrix (teleportGlitch)

    // The Vibe: For when you navigate between pages instantly. Bob disappears in a localized tracking distortion and blips back into reality somewhere else.
    teleportGlitch(dx, dy, duration = 400) {
        return this.enqueue(async () => {
            // console.log("Bob plays teleportGlitch");
            // Phase 1: Disappear via extreme horizontal scale stretching (flatline)
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1, 1)`, opacity: 1 },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(3, 0.05)`, opacity: 0.8 },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(0, 0)`, opacity: 0 }
            ], { duration: duration * 0.4, fill: "forwards" }).finished;

            // Shift internal state coordinates instantly while invisible
            this.x += dx;
            this.y += dy;

            // Phase 2: Materialize at target site
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(0, 0)`, opacity: 0 },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(0.1, 2)`, opacity: 0.8 },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1, 1)`, opacity: 1 }
            ], { duration: duration * 0.6, fill: "forwards" }).finished;
        });
    }



    // The SARGable Target Scan (radarSweep)

    // The Vibe: Scanning the page layout for indexes.Bob tilts aggressively forward and projects an imaginary tracking cone across the DOM before returning to zero.
    radarSweep(duration = 1800) {
        return this.enqueue(async () => {
            // console.log("Bob plays radarSweep");
            // Lean forward into tracking position
            const scanAngle = this.scaleX < 0 ? 25 : -25;

            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${scanAngle}deg) scaleX(${this.scaleX})`, easing: "ease-out" }
            ], { duration: 300, fill: "forwards" }).finished;

            // Pan back and forth slowly searching for predicates
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${scanAngle}deg) scaleX(${this.scaleX})` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${scanAngle * 1.5}deg) scaleX(${this.scaleX})` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${scanAngle * 0.5}deg) scaleX(${this.scaleX})` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${scanAngle}deg) scaleX(${this.scaleX})` }
            ], { duration: duration - 600, easing: "ease-in-out" }).finished;

            // Snap back straight
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${scanAngle}deg) scaleX(${this.scaleX})` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})`, easing: "ease-in" }
            ], { duration: 300, fill: "forwards" }).finished;
        });
    }

    floorIt(speedMultiplier = 1) {
        return this.enqueue(async () => {
            // console.log("Bob plays floorIt");
            const originalTransformOrigin = this.el.style.transformOrigin;
            this.el.style.transformOrigin = "bottom center";

            // --- 1. REAL HARD STRETCH (Stretching that gas pedal foot) ---
            // Bob hinges backward and sticks one foot way out forward to prep
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(0deg) scale(1, 1)` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(-20deg) scale(1.2, 0.8)`, easing: "ease-out" }
            ], { duration: 700, fill: "forwards" }).finished;

            // --- 2. REV THE ENGINE (Vibrating with mechanical tension) ---
            for (let i = 0; i < 4; i++) {
                await this.el.animate([
                    { transform: `translate3d(${this.x}px, ${this.y - 2}px, 0) rotate(-20deg) scale(1.2, 0.8)` },
                    { transform: `translate3d(${this.x}px, ${this.y + 2}px, 0) rotate(-23deg) scale(1.25, 0.78)` }
                ], { duration: 75 }).finished;
            }

            // --- 3. THE LAUNCH (Drop the clutch!) ---
            // He snaps far forward, squashing flat into supersonic speed contours
            const targetLeftEdge = -300; // Blast completely off the left side of the container

            // await this.el.animate([
            //     {
            //         transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(-20deg) scale(1.2, 0.8)`,
            //         offset: 0
            //     },
            //     {
            //         transform: `translate3d(${this.x - 40}px, ${this.y + 4}px, 0) rotate(15deg) scale(1.4, 0.5)`,
            //         offset: 0.15,
            //         easing: "ease-in"
            //     },
            //     {
            //         transform: `translate3d(${targetLeftEdge}px, ${this.y}px, 0) rotate(10deg) scale(2, 0.4)`,
            //         offset: 1,
            //         easing: "linear"
            //     }
            // ], { duration: 500 / speedMultiplier, fill: "forwards" }).finished;

            // --- 4. THE DRIFT RETURN ---
            // Pause out of bounds, then leisurely stroll back to his starting coordinates from the right
            await new Promise(r => setTimeout(r, 600));

            this.el.style.transformOrigin = originalTransformOrigin;

            // Pop him back on the right side and skate smoothly back to baseline
            await this.el.animate([
                { transform: `translate3d(${this.x + 300}px, ${this.y}px, 0) scale(1, 1)` },
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) scale(1, 1)`, easing: "ease-out" }
            ], { duration: 800, fill: "forwards" }).finished;
        });
    }
    // The Gravity Drop (cliffFall)

    // The Vibe: Bob accidentally slips off the edge of whatever Radzen card or widget container he was climbing and tumbles straight down with simulated acceleration until hitting the floor bounds.
    cliffFall(fallDuration = 200) {
        return this.enqueue(async () => {
            // console.log("Bob plays cliffFall");
            // If he is already at the base (y === 0), nothing happens
            if (this.y >= 0) return;

            const targetY = 0; // Baseline floor anchor
            const tumbleTurns = 2;

            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${this.y}px, 0) rotate(${this.rotation}deg) scaleX(${this.scaleX})` },
                {
                    transform: `translate3d(${this.x}px, ${targetY}px, 0) rotate(${this.rotation + (360 * tumbleTurns)}deg) scaleX(${this.scaleX})`,
                    easing: "cubic-bezier(0.47, 0, 0.745, 0.715)" // Accelerating gravity curve
                }
            ], { duration: fallDuration, fill: "forwards" }).finished;

            // Compression impact bounce against floor bounds
            await this.el.animate([
                { transform: `translate3d(${this.x}px, ${targetY}px, 0) scale(1.4, 0.5)` },
                { transform: `translate3d(${this.x}px, ${targetY - 15}px, 0) scale(0.9, 1.1)`, easing: "ease-out" },
                { transform: `translate3d(${this.x}px, ${targetY}px, 0) scale(1, 1)`, easing: "ease-in" }
            ], { duration: 300, fill: "forwards" }).finished;

            this.y = targetY;
            this.rotation += (360 * tumbleTurns);
        });
    }



    initRandomCostumes(intervalMs = 10000) { // Default to every 15 seconds
        setInterval(() => {
            // Core Guard: Don't change outfits if Bob is actively performing an animation sequence
            if (this.queue && this.queue.length > 0) {
                return;
            }
            
            // Roll a random number between 1 and 5
            var idx = Math.floor(Math.random() * 8) + 1;

            // Update the source image
            this.el.src = '../img/pacman/pacman_ghost_' + idx + '.png';

            // Optional: Trigger a tiny little "shiver" or "hop" so the user notices the outfit swap!
            this.rock(10, 150, 1);

            // console.log("Bob plays cliffFall");
        }, intervalMs);
    }




    /**
     * Map play commands safely from window.bob proxy wrapper
     */
    play(name) {
        switch (name) {
            case "PanicLeft":
                return this.panicRoll(-200);
            case "PanicRight":
                return this.panicRoll(200);
            case "Lift":
                return this.heavyLift();
            case "GlitchLeft":
                return this.teleportGlitch(-200, -50);
            case "GlitchRight":
                return this.teleportGlitch(200, -50);
            case "Scan":
                return this.radarSweep();
            case "Fall":
                return this.cliffFall();
            case "Test":
                return this.hop(30);
            case "Idle":
                return this.rock(10, 150, 1);
            case "JumpEscalation":
                return this.playJumpEscalation();
            case "BounceLeft":
                return this.playBoundaryBounce("left");
            case "NavigatorClimb":
                return this.playNavigatorClimb();
            case "EverestNavigatorClimb":
                return this.playEverestNavigatorClimb();
            case "FujiNavigatorClimb":
                return this.playFujiNavigatorClimb();
            case "FloorIt":
                return this.floorIt(.2);
            case "BounceLeft":
                return this.playBoundaryBounce("left");
            case "BounceRight":
                return this.playBoundaryBounce("right");
            case "Radar":
                return this.radarSweep();
            case "Rock":
                return this.rock(15, 150, 10);
            case "Lift":
                return this.heavyLift();
            default:
                // console.log("Bob doesn't handle action:", name);
                return Promise.resolve();
        }
    }
}