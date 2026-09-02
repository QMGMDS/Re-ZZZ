namespace GamePlay.Character
{
    public struct CharacterActionState
    {
        private string _currentActionId;
        private float _logicalProgressSeconds;
        private CharacterIntention _intention;
        private CharacterFact _fact;

        public string CurrentActionId => _currentActionId;
        public float LogicalProgressSeconds => _logicalProgressSeconds;
        public CharacterIntention Intention => _intention;
        public CharacterFact Fact => _fact;

        public CharacterActionState(string currentActionId)
        {
            _currentActionId = currentActionId;
            _logicalProgressSeconds = 0f;
            _intention = CharacterIntention.AllFalse;
            _fact = CharacterFact.AllFalse;
        }

        #region 动作状态的修改方法

        public void SetCurrentActionId(string currentActionId)
        {
            _currentActionId = currentActionId;
        }

        public void SetLogicalProgressSeconds(float logicalProgressSeconds)
        {
            _logicalProgressSeconds = logicalProgressSeconds;
        }

        public void SetIntention(CharacterIntention intention)
        {
            _intention = intention;
        }

        public void SetFact(CharacterFact fact)
        {
            _fact = fact;
        }

        #endregion
    }
}
