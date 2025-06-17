-- main.lua
local mod      = RegisterMod("MyQuizMod", 1)
local game     = Game()

-- 이전 방 이름 저장
local lastRoomName = nil

-- 모드 상태 변수
local quizRunning, zombieRunning, mazeRunning = false, false, false
local quizTimer, quizQuestion, quizAnswer, quizInput = 0, "", "", ""
local zombieTimer, noShoot = 0, false
local mazeTimer = 0

-- 시작 함수들
local function startQuiz()
    quizRunning, zombieRunning, mazeRunning = true, false, false
    quizTimer, quizQuestion, quizAnswer, quizInput = 10, "2 + 3 = ?", "5", ""
    game:GetHUD():ShowItemText("Quiz Started!")
    print("[MyQuizMod] → QuizRoom started")
end

local function startZombie()
    zombieRunning, quizRunning, mazeRunning = true, false, false
    zombieTimer, noShoot = 10, true
    game:GetHUD():ShowItemText("Zombie Started!")
    print("[MyQuizMod] → ZombieRoom started")
end

local function startMaze()
    mazeRunning, quizRunning, zombieRunning = true, false, false
    mazeTimer = 60  -- 1분
    game:GetHUD():ShowItemText("Maze Started! Find the door in 60s")
    print("[MyQuizMod] → MazeRoom started")
end

-- 방 이름 변경 감지 및 모드 토글
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
            elseif name == "MazeRoom" then
                startMaze()
            else
                quizRunning, zombieRunning, mazeRunning, noShoot = false, false, false, false
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
                (room:GetDoor(0) or { Open=function() end }):Open(false)
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
            (room:GetDoor(0) or { Open=function() end }):Open(false)
            zombieRunning, noShoot = false, false
        end
    end

    -- 미로 진행: 문 열림 감지 및 타임아웃
    if mazeRunning then
        mazeTimer = mazeTimer - 1/30
        -- 문이 열리면 성공
        for slot = 0, 3 do
            local d = room:GetDoor(slot)
            if d and d:IsOpen() then
                game:GetHUD():ShowItemText("Maze Cleared!")
                mazeRunning = false
                break
            end
        end
        -- 시간이 다 되면 죽음
        if mazeRunning and mazeTimer <= 0 then
            game:GetHUD():ShowItemText("Maze Failed! You died.")
            player:TakeDamage(9999, DamageFlag.DAMAGE_RED_HEARTS, EntityRef(player), 0)
            mazeRunning = false
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
        local txt1 = quizQuestion .. " (" .. math.ceil(quizTimer) .. "s)"
        local txt2 = "Answer: " .. quizInput
        local x1 = screenWidth - #txt1 * 6 - margin
        local x2 = screenWidth - #txt2 * 6 - margin
        Isaac.RenderText(txt1, x1, 20, 1,1,1,255)
        Isaac.RenderText(txt2, x2, 40, 1,1,1,255)

    elseif zombieRunning then
        local txt = "Survive: " .. math.ceil(zombieTimer) .. "s"
        local x = screenWidth - #txt * 6 - margin
        Isaac.RenderText(txt, x, 20, 1,1,1,255)

    elseif mazeRunning then
        local txt = "Maze: " .. math.ceil(mazeTimer) .. "s"
        local x = screenWidth - #txt * 6 - margin
        Isaac.RenderText(txt, x, 20, 1,1,1,255)
    end
end)
