# **LLM Valley – Game Design Document (Core Systems)**

## **1\. Core Gameplay Loop**

The game progresses in a repeating day-based loop where the player manages time, stamina, and relationships to maximize efficiency and progression.

### **Loop Structure**

1. **Day Start**  
   * The player wakes up with **full stamina**.  
   * The in-game clock begins.  
   * NPC schedules and shop availability are initialized.  
   * Crop growth advances based on the day/night cycle.  
2. **Resource Gathering (Primary Activities)**  
   * **Farming**  
     * Prepare soil using a hoe.  
     * Plant seeds.  
     * Water crops daily.  
     * Harvest when growth conditions are met.  
   * **Fishing**  
     * Fish at the lake using a fishing rod.  
     * Engage in a timing/precision-based minigame.  
   * Both activities **consume stamina**.  
3. **Economic Phase**  
   * Sell harvested crops and caught fish to the **Town Merchant**.  
   * Earn currency.  
   * Improve relationship with merchant through transactions.  
4. **Investment Phase**  
   * Spend money to:  
     * Purchase **tools** (Tool Shop)  
     * Buy **seeds** (Seed Shop)  
     * Unlock **new farm areas** (Builder)  
   * Shop inventories and unlocks are tied to **relationship progression**.  
5. **Social Phase**  
   * Interact with NPCs (limited to **once per day per NPC**).  
   * Engage in AI-driven conversations.  
   * Increase relationship values:  
     * **Trust**  
     * **Friendship**  
     * **Love**  
   * Unlock:  
     * Items  
     * Shop tiers  
     * New interactions and dialogue paths  
6. **End Day**  
   * Player returns home and sleeps.  
   * Day resets and progression systems update.

---

## **2\. Mechanics**

### **2.1 Controls and Player Interaction**

* **Viewpoint**  
  * Fixed, angled top-down camera (\~35°), similar to Stardew Valley.  
* **Movement**  
  * Player movement via **WASD keys**.  
  * Movement is smooth and grid-independent.  
* **Item Usage**  
  * **Left Mouse Button (LMB):**  
    * Use currently equipped tool in facing direction.  
  * Tools interact contextually (soil, crops, water, fish zones).  
* **Interaction Input**  
  * **E Key:**  
    * Interact with NPCs, buildings, shops, and special objects.  
* **Inventory System**  
  * Capacity: **10 item slots**  
  * Players can:  
    * Select active item  
    * Swap between tools and resources  
  * Inventory supports:  
    * Crops  
    * Fish  
    * Seeds  
    * Tools

---

### **2.2 AI Chat System (Core Feature)**

* NPCs are powered by **dynamic AI dialogue systems**.  
* Features:  
  * Free-form player input (text-based).  
  * Context-aware responses.  
  * Persistent memory of past interactions.  
* Outcomes:  
  * Relationship progression  
  * Unlockable content:  
    * Items  
    * Shop upgrades  
    * Farm expansions  
  * Behavioral changes in NPCs

---

### **2.3 Farming System**

* Primary income source.  
* Workflow:  
  * Till soil using Hoe  
  * Plant seeds  
  * Water crops daily  
  * Harvest when fully grown  
* Crop Variables:  
  * Seed cost  
  * Sell value  
  * Growth duration (in days)

---

### **2.4 Fishing System**

* Secondary income source.  
* Location: Farm lake  
* Mechanic:  
  * Timing/precision minigame  
  * Player must keep fish within a target zone to fill a catch meter  
* Fish Variables:  
  * Rarity  
  * Sell value

---

### **2.5 Day-Night & Stamina System**

* Time progresses continuously during the day.  
* Player has a finite **stamina pool**:  
  * Actions (farming, fishing) consume stamina.  
* At day end:  
  * Stamina resets  
  * Crop growth updates  
  * NPC states refresh

---

### **2.6 Relationship Progression System**

* Core progression driver.  
* Each NPC has 3 relationship metrics:  
  * **Trust**  
  * **Friendship**  
  * **Love**  
* Progression Effects:  
  * Unlock new shop tiers  
  * Unlock new areas  
  * Enable advanced interactions  
  * Modify dialogue behavior

---

## **3\. Game Content**

### **3.1 Crops**

* Total: **12 crops**  
* Unlock Structure:  
  * 3 tiers (4 crops each)  
  * Unlocks tied to **Shopkeeper Trust level**  
* Each crop has:  
  * Seed  
  * Growth time  
  * Sell value

---

### **3.2 Fish**

* Total: **6 fish types**  
* Differentiated by:  
  * Rarity  
  * Sell value

---

### **3.3 Tools**

* Types:  
  * Hoe  
  * Watering Can  
  * Fishing Rod  
* Tiers:  
  * Wood  
  * Copper  
  * Iron  
* Tier Effects:  
  * Increased durability  
  * Improved efficiency

---

## **4\. World Structure**

![][image1]

### **Scenes**

#### **Farm Scene**

* Player House  
* Lake (Fishing Area)  
* 4 Farming Fields  
  * Initially: 1 unlocked  
* Pathway to Town

#### **Town Scene**

* Town Square  
* Statue  
* Seed Shop  
* Tool Shop  
* Builder’s Shop  
* Merchant Stand  
* Pathway to Farm  
* Core loop requires **frequent traversal** between Farm and Town.

---

## **5\. Shops and NPC Systems**

### **Seed Shop**

* Sells crop seeds  
* Progression:  
  * Starts with 4 seeds  
  * Unlocks additional sets via **Trust thresholds**  
  * Total: 12 seeds

---

### **Tool Shop (Craftsman)**

* Sells tools and upgrades  
* Tools have 3 tiers (Wood → Copper → Iron)  
* Unlocks tied to **relationship progression**

---

### **Builder’s Shop**

* Unlocks additional farming fields  
* Requires:  
  * Currency  
  * Trust level thresholds

---

### **Town Merchant**

* Buys crops and fish  
* Daily sell limit:  
  * Base limit  
  * Increases with **Trust progression**

---

## **6\. Narrative Setup**

The player inherits a neglected farm in LLM Valley from their late grandfather, a once-respected but eccentric figure known for his experiments with conversational machines and strong ties to the townsfolk. As the player restores the farm, they also revive his unfinished work—rebuilding relationships, uncovering hidden systems, and shaping the future of the valley through AI-driven interactions.

[image1]: <data:image/png;base64,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27alzz//nBISEigrK4vS09PFYw5cmkqVKokQFk64fVANQQsAADzhwIEDFUJQsJaUlCS/NCLy+gLbxIkT5cUdJ1xI6d27Nz311FPlpvHo0/HjxykzM5OOHj0qpvHjjIwM/3wepQo0atQo6tevn3g8a9YsEbo4TO3bt09My8vLE6NbBw8epMqVK4tpqamplJubK5bVnufk5IjHHNS01xYXF4vt/+hv3ejUqVNiGgc2bVnVELQAAMA15GAjNzuQt8lu26cnXNDq27cvnX766VS9enX68MMPacOGDbRo0SL67bffaMuWLWL0idWqVYt+/fVXWrVqFZ08eVJM55Ck4aDF69LUrl2btm3bJtbNHnnkERHEjhw5IoJV9+7daeTIkWLe2WefTT179qQHHnhAPF++fLkIUWPGjKH+/fuLkbDOLS+iOY/3E++bnZ1Nt9xyC3366af+91MJQQsA4oZ/op0/f774iTaYO+64Q/w0Gws+efMIxeLFi8UJ1ojVq1fTsWPH5MlR4U5kxYoV8mSIktFRKa05RbD9sqtwQUse0eJLeBySWrVqJZ7XqVNHBBptFOryyy+ndu3a6QYtXp7vxdKC1qOPPuoPWnx5kS9XLliwQMzjkMfhi88t7LLLLqMOHTqIS5m33367OA883PXPYh/4fXmUDUELAFyHh/X5JPfmm2/SrbfeKobzZVdffTW988478mTDvvrqK3GC/uc//0mvvPIKXX/99fIiQfGJPzExUZ4cFf6JvWnTpvJkiFCwICI3Xsbp5P204z6FC1q9evUSo00cmvjy3i+//EKFhYXi/zrvywsvvCCC0bBhw8TyPF07FwQGLR6N4nXxtHvvvZcaNGggLi/ycnzJsEmTJjRgwAARtPj/OP8gxevl8wgHLX7O92pp73Ho0CG67bbbRHjjoDX6tksRtADA3finTz4R8slYw8P9NWrUoOeff148v+aaa2jo0KFUv359eu6558S0bt26iZ9kmzdv7r/HY+rUqeJSxF133eVfF+Offnl0SrZ+/XqxPAcwxidr/qmXT958LwmfjH/66Sd6+OGHxU/jfOlD8/e//50mTZokHvOJnj344INiuznMMe5EXnvtNbr00kspLS2NunTpor0cIpScnFwueHiJXfc7XNCaPHky3XTTTXTdddfRwIEDxYjueeedJ6az/Px8EW7WrFkjnn/99dfiByBe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>