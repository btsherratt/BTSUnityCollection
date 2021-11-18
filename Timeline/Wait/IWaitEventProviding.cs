using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWaitEventProviding {
    public delegate void WaitEventAction();

    WaitEventAction WaitEvent { get; set; }
}
