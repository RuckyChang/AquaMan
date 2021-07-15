using AquaMan.Domain.Entity;
using System;
using System.Collections.Generic;

namespace AquaMan.Domain
{
    public enum BulletOrderStateType
    {
        FIRED,
        HIT
    }

    public class BulletOrderState
    {
        public BulletOrderStateType StateType { get; }
        public DateTime OccurredAt { get; }
        public BulletOrderState(BulletOrderStateType state)
        {
            StateType = state;
            OccurredAt = DateTime.UtcNow;
        }
    }

    public class HitTarget
    {
        public string ID { get; }
        // TODO: other information
        public DateTime OccurredAt { get; }

        public HitTarget(string id)
        {
            ID = id;
            OccurredAt = DateTime.UtcNow;
        }
    }

    public class BulletOrder
    {
        public string ID { get; }
        public string AgentId { get; }
        public string GameId { get; }
        public string GameRoomID { get; }
        public string AccountID { get; }
        public BulletOrderState CurrentState { get; }
        public List<BulletOrderState> StateLogs { get; }
        public List<HitTarget> HitTargets { get; } = new List<HitTarget>();

        public Cost Cost { get; }

        public BulletOrder(
            string id,
            string agentId,
            string gameId,
            string gameRoomId,
            string accountId,
            BulletOrderState currentState,
            List<BulletOrderState> stateLogs,
            Cost cost
            )
        {
            ID = id;
            AgentId = agentId;
            GameId = gameId;
            GameRoomID = gameRoomId;
            AccountID = accountId;
            CurrentState = currentState;
            StateLogs = stateLogs;
            Cost = cost;
        }

        public void OnHit(HitTarget hitTaget)
        {
            HitTargets.Add(hitTaget);
        }
    }
}
