-- main.lua
local mod      = RegisterMod("MyQuizMod", 1)
local game     = Game()

-- 이전 방 이름 저장용
local lastRoomName = nil

-- 퀴즈/좀비 상태 변수
local quizRunning, zombieRunning = false, false
local quizTimer, quizQuestion, quizAnswer, quizInput = 0, "", "", ""
local zombieTimer, noShoot = 0, false

-- 퀴즈 시작 함수
local function startQuiz()
    quizRunning    = true
    zombieRunning  = false
    quizTimer      = 10
    quizQuestion   = "2 + 3 = ?"
    quizAnswer     = "5"
    quizInput      = ""
    game:GetHUD():ShowItemText("Quiz Started!")
    print("[MyQuizMod] → QuizRoom started")
end

-- 좀비 시작 함수
local function startZombie()
    zombieRunning  = true
    quizRunning    = false
    zombieTimer    = 10
    noShoot        = true
    game:GetHUD():ShowItemText("Zombie Started!")
    print("[MyQuizMod] → ZombieRoom started")
end

-- 방 이름 변경 감지 및 모드 토글, 진행 로직
mod:AddCallback(ModCallbacks.MC_POST_UPDATE, function()
    local level = game:GetLevel()
    local desc  = level:GetCurrentRoomDesc()
    if desc and desc.Data then
        local name = desc.Data.Name or "(none)"
        if name ~= lastRoomName then
            lastRoomName = name
            print(string.format("[MyQuizMod] Entered Room Name = %s", name))
            if name == "QuizRoom" then
                startQuiz()
            elseif name == "ZombieRoom" then
                startZombie()
            else
                quizRunning, zombieRunning, noShoot = false, false, false
            end
        end
    end

    local room   = game:GetRoom()
    local player = game:GetPlayer(0)

    -- 퀴즈 진행
    if quizRunning then
        quizTimer = quizTimer - 1/30
        for k = Keyboard.KEY_0, Keyboard.KEY_9 do
            if Input.IsButtonTriggered(k, 0) then
                quizInput = quizInput .. tostring(k - Keyboard.KEY_0)
            end
        end
        if Input.IsButtonTriggered(Keyboard.KEY_BACKSPACE, 0) then
            quizInput = quizInput:sub(1, -2)
        end
        if Input.IsButtonTriggered(Keyboard.KEY_ENTER, 0) then
            if quizInput == quizAnswer then
                game:GetHUD():ShowItemText("Correct! Clearing enemies…")
                for _, e in ipairs(Isaac.GetRoomEntities()) do
                    if e:IsVulnerableEnemy() then e:Remove() end
                end
                local d = room:GetDoor(0)
                if d then d:Open(false) end
            else
                game:GetHUD():ShowItemText("Wrong! You lose -0.5 heart.")
                player:TakeDamage(1, DamageFlag.DAMAGE_RED_HEARTS, EntityRef(player), 0)
            end
            quizRunning = false
        elseif quizTimer <= 0 then
            game:GetHUD():ShowItemText("Time's up! You died.")
            player:TakeDamage(9999, DamageFlag.DAMAGE_RED_HEARTS, EntityRef(player), 0)
            quizRunning = false
        end
    end

    -- 좀비 진행
    if zombieRunning then
        zombieTimer = zombieTimer - 1/30
        if zombieTimer <= 0 then
            game:GetHUD():ShowItemText("Survived! Clearing enemies…")
            for _, e in ipairs(Isaac.GetRoomEntities()) do
                if e:IsVulnerableEnemy() then e:Remove() end
            end
            local d = room:GetDoor(0)
            if d then d:Open(false) end
            zombieRunning, noShoot = false, false
        end
    end
end)

-- 좀비 모드: 사격 차단
mod:AddCallback(ModCallbacks.MC_POST_FIRE_TEAR, function(_, tear)
    if noShoot then tear:Remove() end
end)

-- HUD 렌더링: 우측 정렬된 타이머 & 입력 표시
mod:AddCallback(ModCallbacks.MC_POST_RENDER, function()
    local screenWidth = 320
    local margin = 10

    if quizRunning then
        local text1 = quizQuestion .. " (" .. math.ceil(quizTimer) .. "s)"
        local text2 = "Answer: " .. quizInput
        local x1 = screenWidth - #text1 * 6 - margin
        local x2 = screenWidth - #text2 * 6 - margin

        Isaac.RenderText(text1, x1, 20, 1, 1, 1, 255)
        Isaac.RenderText(text2, x2, 40, 1, 1, 1, 255)

    elseif zombieRunning then
        local text = "Survive: " .. math.ceil(zombieTimer) .. "s"
        local x = screenWidth - #text * 6 - margin

        Isaac.RenderText(text, x, 20, 1, 1, 1, 255)
    end
end)
