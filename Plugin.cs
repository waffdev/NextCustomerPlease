using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using MyBox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NextCustomerPlease
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        private KeyCode _keybind;
        private ConfigEntry<KeyCode> configKey;

        private void Awake()
        {
            configKey = Config.Bind("Next Customer Please", "KeyBind", KeyCode.X, "The keybind that will call a customer to your checkout.");
            _keybind = configKey.Value;

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update()
        {
            if (Input.GetKeyDown(_keybind))
            {
                CustomerManager customerManager = Singleton<CustomerManager>.Instance;
                CheckoutManager checkoutManager = Singleton<CheckoutManager>.Instance;
                List<Customer> waitingCustomers = customerManager.m_AwaitingCustomers;

                Checkout playerCheckout = null;

                // Get player checkout

                PlayerInteraction playerInteraction = Singleton<PlayerInteraction>.Instance;
                if (playerInteraction.m_InteractableOnUseStarted is Checkout){
                    playerCheckout = (Checkout)playerInteraction.m_InteractableOnUseStarted;
                }

                if (playerCheckout == null)
                {
                    Debug.Log("Player Checkout null");
                    return;
                }

                Customer customerToMove = null;
                int queueLength = 0;

                // Find the longest queue and take a customer off the end of it
                checkoutManager.m_Checkouts.ForEach(checkout =>
                {
                    if (checkout != playerCheckout && !checkout.m_IsSelfCheckout)
                    {
                        if (checkout.m_Customers.Count > 1)
                        {
                            if (checkout.m_Customers.Count >= queueLength)
                            {
                                customerToMove = checkout.m_Customers.Last();
                                queueLength = checkout.m_Customers.Count;
                            }
                        }
                    }
                });


                if ((playerCheckout.m_Customers.Count <= playerCheckout.m_Queue.m_QueueLength)) // Check if there's room to move the customer
                {
                    customerToMove.m_Checkout.Unsubscribe(customerToMove);
                    playerCheckout.Subscribe(customerToMove);
                    Singleton<SFXManager>.Instance.PlayCheckoutWarningSFX();
                }

                
            }
        }
    }
}
