﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kingsland.PiFaceSharp.PinControllers
{
    /// <summary>
    /// Class for tracking the state of an PiFace input pin via interrupt service routines.
    /// </summary>
    /// <remarks>
    /// PiFace device must be ISR enabled.
    /// </remarks>
    public class InputPinController : PinControllerBase
    {

        #region fields
        
        private byte _inputPin;
        private bool _state;
        private int _gateDuration;
        private DateTime _lastChangeToTrueAt = DateTime.MinValue;

        #endregion

        #region events

        public event EventHandler<PinChangedEventArgs> PinChanged;

        #endregion

        #region constructors

        /// <summary>
        /// Creates an instance of InputPinController.
        /// </summary>
        /// <param name="piface">PiFace device. Must be ISR enabled.</param>
        /// <param name="inputPin">Input pin to track changes.</param>
        /// <param name="gateDuration">Gate time in miliseconds in within further changes are ignored (antibeat function, default 20ms)</param>
        public InputPinController(IISRPiFaceDevice piface, byte inputPin, int gateDuration = 20) 
            : base(piface) 
        {
            if (inputPin > 7)
            {
                throw new ArgumentOutOfRangeException("inputPin", "inputPin must be in the range 0-7");
            }
            if (gateDuration < 0)
            {
                throw new ArgumentOutOfRangeException("gateDuration", "gateDuration must be positive");
            }
            if (!piface.IsISREnabled)
            {
                throw new ArgumentOutOfRangeException("piface", "piface must be ISR enabled");
            }
            this._inputPin = inputPin;
            this._gateDuration = gateDuration;
            this._state = piface.GetInputPinState(inputPin);
            piface.InputsChanged += PiFace_InputsChanged;
        }

        #endregion

        #region properties

        public new IISRPiFaceDevice PiFace
        {
            get
            {
                return (IISRPiFaceDevice)base.PiFace;
            }
        }

        public byte InputPin
        {
            get
            {
                return _inputPin;
            }
        }

        public int GateDuration
        {
            get
            {
                return _gateDuration;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("GateDuration", "GateDuration must be positive");
                }
                _gateDuration = value;
            }
        }

        public bool State
        {
            get
            {
                return _state;
            }
            private set
            {
                _state = value;
            }
        }

        #endregion

        #region methods

        private void PiFace_InputsChanged(object sender, InputsChangedEventArgs e)
        {
            int pinMask = (1 << this.InputPin);
            bool state = ((e.InputPinStates & pinMask) == 0);
            DateTime now = DateTime.Now;

            // if state is changed and time since last change is greater than GateDuration
            // (store and check GateDuration only on change from state false to true,
            //  to prevent from stucking in half a cycle..)
            if (!this.State && state &&
                now.Subtract(_lastChangeToTrueAt).TotalMilliseconds >= this.GateDuration)
            {
                this.State = state;
                _lastChangeToTrueAt = now;
                // raise event
                OnPinChanged(state);
            }
            else if (this.State && !state)
            {
                this.State = state;
                // raise event
                OnPinChanged(state);
            }
        }

        protected virtual void OnPinChanged(bool state)
        {
            if (PinChanged != null)
            {
                PinChanged(this, new PinChangedEventArgs(this.InputPin, state));
            }
        }

        #endregion

    }
}
