-- main.lua
local mod      = RegisterMod("MyQuizMod", 1)
local gameInst = Game()

--

-- 모드 활성화 플래그
local quizModeActive, zombieModeActive = false, false
-- 모드 진행 플래그
local quizRunning, zombieRunning       = false, false
-- 퀴즈 관련 상태
local quizQuestion, quizAnswer, quizTimer, quizInput = "", "", 0, ""
-- 좀비 관련 상태
local zombieTimer, noShoot             = 0, false

-- Q/Z 키로 모드 토글
mod:AddCallback(ModCallbacks.MC_POST_UPDATE, function()
  if Input.IsButtonTriggered(Keyboard.KEY_Q, 0) then
    quizModeActive   = true
    zombieModeActive = false
    quizRunning      = false
    quizInput        = ""
    print("[MyQuizMod] Quiz mode armed")
  end
  if Input.IsButtonTriggered(Keyboard.KEY_Z, 0) then
    zombieModeActive = true
    quizModeActive   = false
    zombieRunning    = false
    print("[MyQuizMod] Zombie mode armed")
  end
end)

-- 방 입장 시—일반 방만
local firstSkip = false
mod:AddCallback(ModCallbacks.MC_POST_NEW_ROOM, function()
  local room = gameInst:GetRoom()
  local idx  = gameInst:GetLevel():GetCurrentRoomIndex()
  if not firstSkip then firstSkip = true return end
  if room:GetFrameCount() ~= 0 then return end
  if room:GetType() ~= RoomType.ROOM_DEFAULT then return end

  -- 퀴즈 시작
  if quizModeActive and not quizRunning then
    quizRunning   = true
    quizTimer     = 10
    quizQuestion  = "2 + 3 = ?"
    quizAnswer    = "5"
    quizInput     = ""
    print("[MyQuizMod] Quiz started in room", idx)
  end

  -- 좀비 시작
  if zombieModeActive and not zombieRunning then
    zombieRunning = true
    zombieTimer   = 10
    noShoot       = true
    print("[MyQuizMod] Zombie survival started in room", idx)
  end
end)

-- 사격 차단 (좀비 모드)
mod:AddCallback(ModCallbacks.MC_POST_FIRE_TEAR, function(_, tear)
  if noShoot then tear:Remove() end
end)

-- 매 프레임 업데이트: 타이머, 입력 처리, 문 열림 감지
mod:AddCallback(ModCallbacks.MC_POST_UPDATE, function()
  local room = gameInst:GetRoom()

  -- 퀴즈 진행
  if quizRunning then
    -- 1) 타이머 감소
    quizTimer = quizTimer - 1/30

    -- 2) 숫자 키 입력 수집
    for k = Keyboard.KEY_0, Keyboard.KEY_9 do
      if Input.IsButtonTriggered(k, 0) then
        quizInput = quizInput .. tostring(k - Keyboard.KEY_0)
      end
    end
    -- 백스페이스 처리
    if Input.IsButtonTriggered(Keyboard.KEY_BACKSPACE, 0) then
      quizInput = quizInput:sub(1, -2)
    end

    -- 3) Enter 로 제출
    if Input.IsButtonTriggered(Keyboard.KEY_ENTER, 0) then
      if quizInput == quizAnswer then
        -- 정답 처리
        gameInst:GetHUD():ShowItemText("Correct! Clearing enemies…", 2)
        for _, e in ipairs(Isaac.GetRoomEntities()) do
          if e:IsVulnerableEnemy() then e:Remove() end
        end
        local d = room:GetDoor(0)
        if d then d:Open(false) end

        quizRunning    = false
        quizModeActive = false

      else
        -- 오답 처리: 입력만 초기화, 계속 시도 가능
        gameInst:GetHUD():ShowItemText("Wrong! Try again.", 2)
        quizInput = ""
      end

    -- 4) 시간초과 처리
    elseif quizTimer <= 0 then
      gameInst:GetHUD():ShowItemText("Time up! Door stays closed", 2)
      quizRunning    = false
      quizModeActive = false
    end
  end

  -- 좀비 생존 진행
  if zombieRunning then
    zombieTimer = zombieTimer - 1/30
    if zombieTimer <= 0 then
      gameInst:GetHUD():ShowItemText("Time's up! Clearing enemies…", 2)
      for _, e in ipairs(Isaac.GetRoomEntities()) do
        if e:IsVulnerableEnemy() then e:Remove() end
      end
      local d = room:GetDoor(0)
      if d then d:Open(false) end
      zombieRunning    = false
      zombieModeActive = false
      noShoot          = false
    end
  end

  -- 문 열림 감지 → 모드 완전 종료
  if quizRunning or zombieRunning then
    for slot = 0, 3 do
      local d = room:GetDoor(slot)
      if d and d:IsOpen() then
        quizRunning      = false
        quizModeActive   = false
        zombieRunning    = false
        zombieModeActive = false
        noShoot          = false
        break
      end
    end
  end
end)

-- 화면 렌더: 퀴즈 텍스트·입력·타이머·생존 타이머 표시
mod:AddCallback(ModCallbacks.MC_POST_RENDER, function()
  if quizRunning then
    Isaac.RenderText(
      quizQuestion .. " (" .. math.ceil(quizTimer) .. "s)",
      20,  20, 1,1,1,255
    )
    Isaac.RenderText(
      "Answer: " .. quizInput,
      20,  40, 1,1,1,255
    )

  elseif zombieRunning then
    Isaac.RenderText(
      "Survive: " .. math.ceil(zombieTimer) .. "s",
      20,  20, 1,1,1,255
    )

  elseif quizModeActive then
    Isaac.RenderText(
      "Quiz mode armed: enter normal room and type answer",
      20,  20, 1,1,1,255
    )

  elseif zombieModeActive then
    Isaac.RenderText(
      "Zombie mode armed: enter normal room to start timer",
      20,  20, 1,1,1,255
    )
  end
end)
