-- 모드 등록
local MyMod = RegisterMod("My First Mod", 1)

-- 콜백 함수: 방에 들어올 때마다 실행
function MyMod:OnRoomEnter()
    Isaac.ConsoleOutput("[MyMod] 새로운 방에 들어왔습니다!\n")
end

-- 콜백 등록
MyMod:AddCallback(ModCallbacks.MC_POST_NEW_ROOM, MyMod.OnRoomEnter)
