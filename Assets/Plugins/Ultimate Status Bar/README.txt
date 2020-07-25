Thank you for purchasing the Ultimate Status Bar UnityPackage!

If you need some help getting started, please feel free to email us at tankandhealerstudio@outlook.com.

/* ------- < IMPORTANT INFORMATION > ------- */
Within Unity, please go to Window / Tank and Healer Studio / Ultimate Status Bar to access important information on how to get started using the Ultimate Status Bar. There is
a ton of information available to help you get the Ultimate Status Bar into your project as fast as possible. However, if you can't view the in-engine documentation
window, please see the information below.
/* ----- < END IMPORTANT INFORMATION > ----- */


// --- IF YOU CAN'T VIEW THE ULTIMATE STATUS BAR WINDOW, READ THIS SECTION --- //
	// --- HOW TO CREATE --- //
There are several ways to create an Ultimate Status Bar within your scene. Below is a list of ways you can do so.
1.) CREATE FROM SCRATCH
You can create an Ultimate Status Bar from scratch using your own images by going to GameObject / Ultimate UI / Ultimate Status Bar. This will create a base Ultimate Status
Bar GameObject with only the basic objects to get your Ultimate Status Bar started. 
2.) USE A PREFAB
You can also use a prefab from the Ultimate Status Bar Prefabs folder. These prefabs are complete Ultimate Status Bar GameObjects with all the needed components. Simply drag
and drop these prefabs into the Hierarchy window to get them in your scene.
3.) ATTACH THE ULTIMATE STATUS BAR SCRIPT
You could also attach the Ultimate Status Bar script to your own status bar and customize it from there.

	// --- HOW TO REFERENCE --- //
The Ultimate Status Bar is incredibly easy to get implemented into your custom scripts. There are a few ways that you can reference the Ultimate Status Bar through code, and
it all depends on how many different status sections you have created on that particular Ultimate Status Bar. For more information on how to reference the Ultimate Status Bar,
please see the Script Reference section of the Ultimate Status Bar inspector.

For this example, we will create an Ultimate Status Bar for the Player of a simple game. Let's assume that the Player has several different status values that must be displayed.
For this example, the Player will have a Health value, and a Energy value. These will need to be created inside the Status Information section in order to be referenced through
code.

After these have been created, we need to give the Ultimate Status Bar a unique name to be referenced through code. This is done in the Script Reference section located within
the Inspector window. For this example, we are creating this status bar for the Player, so that's what we will name it.

Now that each status has been named, and the Ultimate Status Bar has a unique name that can be referenced, simply copy the code provided inside the Script Reference section for
the desired status. Make sure that the Function option is set to Update Status.

After copying the code that is provided, find the function in your player's health script where your player is receiving damage from and paste the example code into the function.
Be sure to put it after the damage or healing has modified the health value. Of course, be sure to replace the currentValue and maxValue of the example code with your character's
current and maximum health values. Whenever the character's health is updated, either by damage or healing done to the character, you will want to send the new information of the
health's value.

This process can be used for any status that you need to be displayed to the user.