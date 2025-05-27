using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public GameObject[] health_bar;
    public GameObject[] energy_bar;
    public GameObject ammo_state;
    public GameObject another_world;
    public GameObject taskLevel2;
    public GameObject taskLevel12;
    public GameObject helpWindow;

    public void SetHealth(int health) {
        if (health > health_bar.Length) return;
        for (int i = 0; i < health_bar.Length; i++) {
            health_bar[i].SetActive(health > i);
        }
    }

    public void SetEnergy(int energy) {
        if (energy > energy_bar.Length) return;
        for (int i = 0; i < energy_bar.Length; i++) {
            energy_bar[i].SetActive(energy > i);
        }
    }

    public void SetAmmo(bool ammo) {
        ammo_state.SetActive(ammo);
    }

    public void SetAnotherWorld(bool state) {
        another_world.SetActive(state);
    }

    public void SetTaskLevel2(bool state) {
        taskLevel2.SetActive(state);
    }

    public void SetTaskLevel12(bool state) {
        taskLevel12.SetActive(state);
    }

    public void SetHelpWindow(bool state) {
        helpWindow.SetActive(state);
    }
}
