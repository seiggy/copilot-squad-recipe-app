using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Models;

namespace RecipeHub.Api.Data;

/// <summary>
/// Hackathon seed data per <c>docs/data-assessment.md</c> §3.
/// Populates Tags (10), Recipes (12), RecipeSteps (~71), and RecipeTags (~26)
/// idempotently. ShareTokens and Favorites are intentionally left empty.
/// </summary>
public static class SeedData
{
    public static void EnsureSeeded(RecipeDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);

        // Idempotency: if any recipes exist, assume seed has already run.
        if (db.Recipes.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // --- Tags ---------------------------------------------------------
        // Use the exact 10 tags from data-assessment.md §3.1. Insertion order
        // matters for readability only; EF assigns Ids.
        var tagNames = new[]
        {
            "Breakfast",
            "Lunch",
            "Dinner",
            "Dessert",
            "Vegetarian",
            "Vegan",
            "Quick",
            "Italian",
            "Asian",
            "Mexican",
        };

        var tags = tagNames.ToDictionary(name => name, name => new Tag { Name = name });

        // Upsert tags: only add ones that aren't already in the DB (handles the
        // edge case where tags were seeded by a prior partial run).
        var existingTagNames = db.Tags.Select(t => t.Name).ToHashSet(StringComparer.Ordinal);
        foreach (var (name, tag) in tags)
        {
            if (!existingTagNames.Contains(name))
            {
                db.Tags.Add(tag);
            }
            else
            {
                tags[name] = db.Tags.Single(t => t.Name == name);
            }
        }

        db.SaveChanges();

        // --- Recipes ------------------------------------------------------
        var recipes = new List<Recipe>
        {
            BuildRecipe(
                title: "Classic Margherita Pizza",
                description: "A Neapolitan classic: blistered crust, bright San Marzano tomato sauce, fresh mozzarella, and torn basil. Simple ingredients that rely on good technique and a very hot oven.",
                prep: 30, cook: 15, servings: 4, difficulty: Difficulty.Medium,
                ingredients: """
                    500g (4 cups) 00 or bread flour
                    325ml (1 1/3 cups) warm water
                    10g (2 tsp) fine sea salt
                    2g (3/4 tsp) instant yeast
                    400g can whole San Marzano tomatoes, crushed by hand
                    250g fresh mozzarella (preferably fior di latte), torn
                    1 bunch fresh basil
                    Extra-virgin olive oil, for finishing
                    Flaky sea salt, to taste
                    """,
                tagNames: new[] { "Italian", "Dinner" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Whisk flour, salt, and yeast in a large bowl. Add water and mix until a shaggy dough forms.", null),
                    ("Knead on a lightly floured surface for 8-10 minutes until smooth and elastic.", null),
                    ("Cover and let rise at room temperature for 1 hour, then divide into 2 balls and refrigerate 24 hours for best flavor.", null),
                    ("Preheat oven (and pizza stone or steel) to its maximum temperature, at least 500°F / 260°C.", null),
                    ("Stretch one dough ball into a 12-inch round, top with crushed tomatoes, torn mozzarella, and a drizzle of olive oil.", null),
                    ("Bake until the crust is puffed and charred in spots, about 10-12 minutes. Finish with fresh basil, olive oil, and flaky salt.", 12),
                },
                now: now),

            BuildRecipe(
                title: "Fluffy Pancakes",
                description: "Tall, tender buttermilk pancakes with crisp golden edges. Resting the batter while the pan heats is the secret to the signature fluff.",
                prep: 10, cook: 15, servings: 4, difficulty: Difficulty.Easy,
                ingredients: """
                    2 cups all-purpose flour
                    2 tbsp sugar
                    2 tsp baking powder
                    1/2 tsp baking soda
                    1/2 tsp fine salt
                    2 cups buttermilk
                    2 large eggs
                    3 tbsp unsalted butter, melted, plus more for the pan
                    1 tsp vanilla extract
                    Maple syrup, for serving
                    """,
                tagNames: new[] { "Breakfast", "Quick" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Whisk flour, sugar, baking powder, baking soda, and salt in a large bowl.", null),
                    ("In a second bowl, whisk buttermilk, eggs, melted butter, and vanilla until smooth.", null),
                    ("Pour the wet ingredients into the dry and stir just until combined; a few lumps are fine. Let the batter rest for 5 minutes.", null),
                    ("Heat a non-stick skillet or griddle over medium heat and brush with butter.", null),
                    ("Scoop 1/4-cup portions of batter onto the pan. Cook until bubbles form and the edges set, then flip and cook the second side.", 3),
                },
                now: now),

            BuildRecipe(
                title: "Chicken Alfredo Pasta",
                description: "Creamy, cheesy fettuccine with seared chicken. The sauce comes together quickly, so have the pasta water boiling before you start the chicken.",
                prep: 15, cook: 25, servings: 4, difficulty: Difficulty.Medium,
                ingredients: """
                    1 lb (450g) fettuccine
                    2 boneless, skinless chicken breasts
                    1 tbsp olive oil
                    4 tbsp unsalted butter
                    4 cloves garlic, minced
                    1 1/2 cups heavy cream
                    1 1/2 cups finely grated Parmigiano-Reggiano
                    Salt and freshly ground black pepper
                    Chopped parsley, for serving
                    """,
                tagNames: new[] { "Italian", "Dinner" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Bring a large pot of well-salted water to a boil.", null),
                    ("Season the chicken with salt and pepper. Heat olive oil in a skillet over medium-high heat and sear the chicken until golden and cooked through, about 5-6 minutes per side. Rest, then slice.", null),
                    ("Add the fettuccine to the boiling water and cook until al dente, about 10 minutes.", 10),
                    ("Reduce the skillet heat to medium. Melt the butter, add the garlic, and cook until fragrant, about 1 minute.", null),
                    ("Pour in the cream and simmer gently until slightly thickened.", 5),
                    ("Off the heat, whisk in the Parmesan until the sauce is smooth. Season to taste.", null),
                    ("Drain the pasta, toss with the sauce and sliced chicken, and finish with parsley and extra Parmesan.", null),
                },
                now: now),

            BuildRecipe(
                title: "Thai Green Curry",
                description: "Fragrant, coconut-rich curry with chicken and Thai basil. Frying the curry paste in thick coconut cream is what builds the signature depth.",
                prep: 20, cook: 20, servings: 4, difficulty: Difficulty.Medium,
                ingredients: """
                    1 lb (450g) boneless, skinless chicken thighs, sliced
                    2 tbsp Thai green curry paste
                    1 can (400ml) full-fat coconut milk
                    1 cup chicken stock
                    1 Thai eggplant or 1/2 regular eggplant, cubed
                    1 red bell pepper, sliced
                    1 tbsp fish sauce
                    2 tsp palm or brown sugar
                    1 handful Thai basil leaves
                    1 lime, cut into wedges
                    Steamed jasmine rice, for serving
                    """,
                tagNames: new[] { "Asian", "Dinner" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Spoon the thick cream from the top of the coconut milk can into a large saucepan over medium heat.", null),
                    ("Add the curry paste and fry, stirring, until deeply fragrant and the oil splits, 2-3 minutes.", null),
                    ("Add the chicken and stir to coat, cooking for 2-3 minutes until the outside turns opaque.", null),
                    ("Pour in the remaining coconut milk and stock. Add the eggplant, fish sauce, and sugar.", null),
                    ("Simmer gently until the chicken is cooked through and the vegetables are tender.", 15),
                    ("Stir in the bell pepper and Thai basil, taste, and adjust with fish sauce or sugar. Serve over jasmine rice with lime wedges.", null),
                },
                now: now),

            BuildRecipe(
                title: "Avocado Toast",
                description: "A crunchy, creamy five-minute breakfast. Good bread and a ripe avocado do the heavy lifting.",
                prep: 5, cook: 3, servings: 2, difficulty: Difficulty.Easy,
                ingredients: """
                    2 thick slices sourdough or country bread
                    1 ripe avocado
                    1/2 lemon, juiced
                    Flaky sea salt
                    Freshly ground black pepper
                    Red pepper flakes, to taste
                    Extra-virgin olive oil, for drizzling
                    """,
                tagNames: new[] { "Breakfast", "Vegetarian", "Quick" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Toast the bread until deeply golden and crisp.", 3),
                    ("Halve the avocado, remove the pit, and scoop the flesh into a small bowl.", null),
                    ("Mash lightly with a fork. Add the lemon juice, a pinch of salt, and pepper.", null),
                    ("Spread the avocado onto the toast, drizzle with olive oil, and finish with flaky salt and red pepper flakes.", null),
                },
                now: now),

            BuildRecipe(
                title: "Beef Tacos",
                description: "Weeknight-friendly seasoned ground beef tacos with warm corn tortillas and fresh toppings. Toast the tortillas directly over the flame if you have a gas burner.",
                prep: 15, cook: 10, servings: 4, difficulty: Difficulty.Easy,
                ingredients: """
                    1 lb (450g) ground beef (80/20)
                    1 small yellow onion, finely chopped
                    2 cloves garlic, minced
                    2 tsp chili powder
                    1 tsp ground cumin
                    1/2 tsp smoked paprika
                    1/2 tsp dried oregano
                    Salt and pepper
                    8 small corn tortillas
                    Diced tomato, shredded lettuce, cotija cheese, and lime wedges, for serving
                    """,
                tagNames: new[] { "Mexican", "Dinner" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Heat a skillet over medium-high heat. Add the beef and break it up with a wooden spoon.", null),
                    ("Once most of the liquid has cooked off, add the onion and a pinch of salt, and cook until softened.", null),
                    ("Stir in the garlic, chili powder, cumin, paprika, and oregano. Cook until fragrant and the beef is well browned.", 8),
                    ("Warm the tortillas in a dry skillet or directly over a low gas flame for a few seconds per side.", null),
                    ("Fill each tortilla with beef and top with tomato, lettuce, cotija, and a squeeze of lime.", null),
                },
                now: now),

            BuildRecipe(
                title: "Chocolate Lava Cake",
                description: "Individual chocolate cakes with a molten, flowing center. Timing is everything — pull them when the edges are set but the middle still jiggles.",
                prep: 20, cook: 14, servings: 4, difficulty: Difficulty.Hard,
                ingredients: """
                    6 oz (170g) bittersweet chocolate, chopped
                    1/2 cup (113g) unsalted butter, plus more for the ramekins
                    2 tbsp cocoa powder, plus more for dusting
                    2 large eggs
                    2 large egg yolks
                    1/4 cup (50g) granulated sugar
                    Pinch of fine salt
                    2 tbsp all-purpose flour
                    Vanilla ice cream or whipped cream, for serving
                    """,
                tagNames: new[] { "Dessert" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Preheat the oven to 425°F / 220°C. Butter four 6-oz ramekins and dust with cocoa powder, tapping out the excess.", null),
                    ("Melt the chocolate and butter together in a heatproof bowl set over simmering water, stirring until smooth. Let cool slightly.", null),
                    ("In a separate bowl, whisk the eggs, egg yolks, sugar, and salt until thick and pale, about 2 minutes.", null),
                    ("Fold the chocolate mixture into the eggs, then gently fold in the flour until just combined.", null),
                    ("Divide the batter evenly among the ramekins.", null),
                    ("Place the ramekins on a baking sheet and bake until the edges are set but the centers still wobble when nudged.", 14),
                    ("Let rest for 1 minute, then run a knife around the edge of each cake and invert onto a plate.", null),
                    ("Serve immediately with ice cream or whipped cream.", null),
                },
                now: now),

            BuildRecipe(
                title: "Vegetable Stir-Fry",
                description: "Fast, crisp-tender vegetables in a glossy soy-ginger sauce. Have everything prepped before the wok hits the heat.",
                prep: 15, cook: 10, servings: 4, difficulty: Difficulty.Easy,
                ingredients: """
                    2 tbsp neutral oil
                    1 tbsp fresh ginger, minced
                    3 cloves garlic, minced
                    1 broccoli crown, cut into florets
                    1 red bell pepper, sliced
                    1 large carrot, thinly sliced on a bias
                    1 cup snow peas
                    3 tbsp soy sauce or tamari
                    1 tbsp rice vinegar
                    1 tsp toasted sesame oil
                    1 tsp cornstarch mixed with 2 tbsp water
                    Steamed rice, for serving
                    """,
                tagNames: new[] { "Asian", "Vegetarian", "Vegan", "Quick" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Whisk the soy sauce, rice vinegar, sesame oil, and cornstarch slurry in a small bowl.", null),
                    ("Heat a wok or large skillet over high heat until smoking. Add the oil, then the ginger and garlic, and stir for 10 seconds.", null),
                    ("Add the carrot and broccoli and stir-fry until just tender-crisp.", 5),
                    ("Add the bell pepper and snow peas and stir-fry for another minute.", null),
                    ("Pour in the sauce and toss until glossy and slightly thickened. Serve over rice.", null),
                },
                now: now),

            BuildRecipe(
                title: "French Onion Soup",
                description: "Deeply caramelized onions in rich beef broth, topped with a toasted baguette and a blanket of bubbling Gruyère. A patient recipe that rewards every minute.",
                prep: 15, cook: 60, servings: 4, difficulty: Difficulty.Hard,
                ingredients: """
                    3 tbsp unsalted butter
                    1 tbsp olive oil
                    2 1/2 lbs (1.1kg) yellow onions, thinly sliced
                    1 tsp fine salt, plus more to taste
                    1 tsp sugar
                    2 cloves garlic, minced
                    1/2 cup dry white wine
                    2 tbsp all-purpose flour
                    6 cups rich beef stock
                    2 sprigs fresh thyme
                    1 bay leaf
                    4 thick slices baguette, toasted
                    1 1/2 cups grated Gruyère cheese
                    """,
                tagNames: new[] { "Dinner" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Melt the butter with the olive oil in a heavy Dutch oven over medium heat. Add the onions, salt, and sugar and stir to coat.", null),
                    ("Cook the onions, stirring every few minutes, until they are deeply golden brown and jammy. Lower the heat if they threaten to scorch.", 45),
                    ("Add the garlic and cook for 1 minute, then pour in the wine and scrape up any browned bits.", null),
                    ("Sprinkle in the flour and stir for a minute to cook out the raw taste.", null),
                    ("Add the stock, thyme, and bay leaf. Simmer gently for 20-30 minutes to marry the flavors. Season to taste.", null),
                    ("Ladle the soup into oven-safe bowls, float a toasted baguette slice on top, and cover generously with Gruyère.", null),
                    ("Broil until the cheese is melted, bubbling, and browned in spots. Serve immediately.", 3),
                },
                now: now),

            BuildRecipe(
                title: "Berry Smoothie Bowl",
                description: "A thick, spoonable smoothie topped with crunchy granola and fresh fruit. Freeze the banana ahead of time for the best texture.",
                prep: 10, cook: 0, servings: 2, difficulty: Difficulty.Easy,
                ingredients: """
                    1 1/2 cups frozen mixed berries
                    1 frozen banana, broken into chunks
                    1/2 cup Greek yogurt
                    1/4 cup milk or plant milk, plus more as needed
                    1 tbsp honey or maple syrup
                    1/2 cup granola
                    Fresh berries, sliced banana, and chia seeds, for topping
                    """,
                tagNames: new[] { "Breakfast", "Vegetarian", "Quick" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Combine the frozen berries, frozen banana, yogurt, milk, and honey in a high-powered blender.", null),
                    ("Blend on low, scraping down the sides as needed, until thick and spoonable. Add more milk one tablespoon at a time only if necessary.", null),
                    ("Divide between two bowls and smooth the tops with a spoon.", null),
                    ("Top with granola, fresh fruit, and a sprinkle of chia seeds. Serve right away.", null),
                },
                now: now),

            BuildRecipe(
                title: "Homemade Sushi Rolls",
                description: "Classic cucumber-avocado and salmon maki made with properly seasoned sushi rice. Keep your hands and knife damp to prevent sticking.",
                prep: 45, cook: 20, servings: 4, difficulty: Difficulty.Hard,
                ingredients: """
                    2 cups short-grain sushi rice
                    2 1/2 cups water
                    1/4 cup rice vinegar
                    2 tbsp sugar
                    1 tsp fine salt
                    4 sheets nori
                    1/2 lb sushi-grade salmon, cut into long strips
                    1/2 English cucumber, julienned
                    1 ripe avocado, sliced
                    Soy sauce, pickled ginger, and wasabi, for serving
                    """,
                tagNames: new[] { "Asian", "Dinner" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Rinse the rice in several changes of cold water until the water runs clear.", null),
                    ("Combine the rice and water in a saucepan, bring to a boil, then reduce to a low simmer, cover, and cook until tender.", 20),
                    ("Meanwhile, warm the rice vinegar, sugar, and salt just until dissolved.", null),
                    ("Transfer the cooked rice to a wide bowl, drizzle with the vinegar mixture, and fold gently with a spatula while fanning to cool.", null),
                    ("Place a sheet of nori shiny-side down on a bamboo mat. Spread a thin layer of rice over the nori, leaving a 1-inch strip clear at the top.", null),
                    ("Lay strips of salmon, cucumber, and avocado across the middle of the rice.", null),
                    ("Using the mat, roll the nori tightly away from you, pressing to seal. Moisten the bare strip to close the roll.", null),
                    ("Wipe a sharp knife with a damp cloth between cuts and slice each roll into 6-8 pieces. Serve with soy sauce, ginger, and wasabi.", null),
                },
                now: now),

            BuildRecipe(
                title: "Tiramisu",
                description: "Classic no-bake Italian dessert with espresso-soaked ladyfingers and clouds of mascarpone cream. Build it the day before so the flavors have time to settle.",
                prep: 30, cook: 0, servings: 8, difficulty: Difficulty.Medium,
                ingredients: """
                    4 large eggs, separated
                    1/2 cup granulated sugar
                    1 lb (450g) mascarpone, at room temperature
                    1 3/4 cups strong brewed espresso, cooled
                    3 tbsp coffee liqueur (optional)
                    24 ladyfinger cookies (savoiardi)
                    2 tbsp unsweetened cocoa powder
                    Shaved dark chocolate, for serving
                    """,
                tagNames: new[] { "Italian", "Dessert" },
                steps: new (string Instruction, int? Timer)[]
                {
                    ("Whisk the egg yolks and sugar in a heatproof bowl over a pot of simmering water until thick, pale, and warm to the touch. Remove and let cool briefly.", null),
                    ("Whisk the mascarpone into the yolk mixture until smooth.", null),
                    ("In a separate, spotlessly clean bowl, whip the egg whites to stiff peaks. Fold them gently into the mascarpone mixture in two additions.", null),
                    ("Combine the espresso and coffee liqueur in a shallow dish. Dip each ladyfinger briefly, turning to coat without soaking through, and arrange in a single layer in a 9x13 inch dish.", null),
                    ("Spread half of the mascarpone mixture over the ladyfingers. Repeat with a second layer of dipped ladyfingers and the remaining mascarpone.", null),
                    ("Cover and refrigerate for at least 6 hours, ideally overnight. Dust generously with cocoa powder and shaved chocolate just before serving.", null),
                },
                now: now),
        };

        // Attach RecipeTag join rows using the actual Tag entities.
        foreach (var recipe in recipes)
        {
            foreach (var rt in recipe.RecipeTags)
            {
                // Tag reference set during BuildRecipe via closure dictionary.
                // Nothing extra needed here; EF will insert the join row.
            }

            db.Recipes.Add(recipe);
        }

        db.SaveChanges();

        // --- Local helpers ------------------------------------------------
        Recipe BuildRecipe(
            string title,
            string description,
            int prep,
            int cook,
            int servings,
            Difficulty difficulty,
            string ingredients,
            string[] tagNames,
            (string Instruction, int? Timer)[] steps,
            DateTime now)
        {
            // Description includes a trailing "Ingredients" block so the single
            // text field carries both blurb and ingredient list (no dedicated
            // Ingredients table exists in the schema).
            var fullDescription = $"{description}\n\nIngredients:\n{ingredients.Trim()}";

            var recipe = new Recipe
            {
                Title = title,
                Description = fullDescription,
                PrepTimeMinutes = prep,
                CookTimeMinutes = cook,
                Servings = servings,
                Difficulty = difficulty,
                CreatedAt = now,
                UpdatedAt = now,
            };

            for (var i = 0; i < steps.Length; i++)
            {
                recipe.Steps.Add(new RecipeStep
                {
                    StepNumber = i + 1,
                    Instruction = steps[i].Instruction,
                    TimerMinutes = steps[i].Timer,
                });
            }

            foreach (var tagName in tagNames)
            {
                recipe.RecipeTags.Add(new RecipeTag { Tag = tags[tagName] });
            }

            return recipe;
        }
    }
}
