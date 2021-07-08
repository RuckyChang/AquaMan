using System;
using System.Collections.Generic;

namespace AquaMan.Domain
{
    public enum OrderType {
        DEPOSIT,
        WITHDRAW
    }

    public enum OrderStateType
    {
        PENDDING,
        LOCAL_COMMITTED,
        REMOTE_COMMITTED
    }

    public class OrderState
    {
        public OrderStateType StateType { get; }
        public DateTime OccurredAt { get; }

        public OrderState(OrderStateType stateType)
        {
            StateType = stateType;
            OccurredAt = DateTime.Now;
        }
    }


    public class Order
    {
        public string ID { get; }
        public string AgentId { get; }
        public string GameId { get; }
        public string AccountID { get; }
        public uint Amount { get; }
        public OrderType OrderType { get;}
        public List<OrderState> StateLogs { get; }
        public OrderState CurrentState { get; private set; }


        public Order(
            string id,
            string agentId,
            string gameId,
            string accountId,
            uint amount,
            OrderType orderType,
            List<OrderState> stateLogs,
            OrderState currentState
            )
        {
            ID = id;
            AgentId = agentId;
            GameId = gameId;
            AccountID = accountId;
            Amount = amount;
            OrderType = orderType;
            StateLogs = stateLogs;
            CurrentState = currentState;
        }

        public void LocalCommit()
        {
            if(CurrentState.StateType == OrderStateType.LOCAL_COMMITTED)
            {
                return;
            }

            if(CurrentState.StateType != OrderStateType.PENDDING)
            {
                throw new StateNotCorrectException(
                    currentState: CurrentState.StateType,
                    newState: OrderStateType.LOCAL_COMMITTED,
                    expectedState: OrderStateType.PENDDING
                    );
            }

            var newOrderState = new OrderState(OrderStateType.LOCAL_COMMITTED);

            StateLogs.Add(
              newOrderState
                );

            CurrentState = newOrderState;
        }

        public void RemoteCommit()
        {
            if(CurrentState.StateType == OrderStateType.REMOTE_COMMITTED)
            {
                return;
            }

            if(CurrentState.StateType != OrderStateType.LOCAL_COMMITTED)
            {
                throw new StateNotCorrectException(
                    currentState: CurrentState.StateType,
                    newState: OrderStateType.REMOTE_COMMITTED,
                    expectedState: OrderStateType.LOCAL_COMMITTED
                    );
            }

            var newOrderState = new OrderState(OrderStateType.REMOTE_COMMITTED);

            StateLogs.Add(
                newOrderState
            );

            CurrentState = newOrderState;
        }
    }

    public class StateNotCorrectException: Exception
    {
        public OrderStateType CurrentState { get; }
        public OrderStateType NewState { get; }
        public OrderStateType ExpectedState { get; }
        public StateNotCorrectException(OrderStateType currentState, OrderStateType newState, OrderStateType expectedState): base()
        {
            CurrentState = currentState;
            NewState = newState;
            ExpectedState = expectedState;
        }
    }
}
