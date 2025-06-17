// 파일 위치: Project_SSAAC/World/Layouts/RoomLayouts.cs
using System;
using System.Collections.Generic;
using Project_SSAAC.World;

namespace Project_SSAAC.World.Layouts
{
    public static class RoomLayouts
    {
        private static Random _random = new Random();

        // --- 설계도에서 사용하는 기호(Legend)의 의미 ---
        // . : 빈 공간
        // R : 바위 (Rock) - 이동 불가, 공격 방어
        // S : 가시 (Spikes) - 이동 가능, 닿으면 데미지
        // P : 구덩이 (Pit) - 이동 불가, 공격 통과
        // E : 적 생성 위치 (Enemy)

        #region 일반 방(Normal, MiniBoss) 설계도 (문 통로 확보됨)
        private static List<string[]> normalRoomTemplates = new List<string[]>
        {
            // --- 감옥 모양 ---
            new string[]
            {
                "R............R",
                "R.R.R.R.R.R.R.RR",
                "R...E........E.R",
                "...............R", // 좌측 문 통로 확보
                ".......E.......R", // 좌측 문 통로 확보
                "...............R", // 좌측 문 통로 확보
                "RR.R.R.R.R.R.R.R",
                "R...E........E.R",
                "R............R"
            },

            // --- 네 개의 기둥 ---
            new string[]
            {
                "................",
                ".RR...E......RR.",
                ".RR..........RR.",
                ".......E........",
                "................",
                ".......E........",
                ".RR..........RR.",
                ".RR...E......RR.",
                "................"
            },

            // --- 중앙 십자 벽 ---
            new string[]
            {
                "...E... .. ......", // 상단 문 통로 확보
                ".......RR.......",
                ".......RR.......",
                ".. ...RRRR...RR..", // 좌측 문 통로 확보
                "..E.RRRR.E.RR..", // 좌측 문 통로 확보
                ".....RRRR...RR..", // 좌측 문 통로 확보
                ".......RR.......",
                "............E...",
                "....... .. ......"  // 하단 문 통로 확보
            },

            // --- 외곽 순찰로 ---
            new string[]
            {
                "RRRRRR. ..RRRRRRR", // 상단 문 통로 확보
                "R.E............R",
                "R.RRRRRRRRRRRR.R",
                ". P..........P.P", // 좌측 문 통로 확보
                ". P.E....E...P. ", // 좌우 문 통로 확보
                ". P..........P.P", // 좌측 문 통로 확보
                "R.RRRRRRRRRRRR.R",
                "R............E.R",
                "RRRRRR. ..RRRRRRR"  // 하단 문 통로 확보
            },
        };
        #endregion

        #region 퍼즐(Puzzle) 방 설계도 (문 통로 확보됨)
        private static List<string[]> puzzleRoomTemplates = new List<string[]>
        {
            // --- 중앙 구덩이 ---
            new string[]
            {
                "................",
                "................",
                "....RRRRRRRR....",
                "....RPPPPRR....",
                "....RPPPPRR....",
                "....RPPPPRR....",
                "....RRRRRRRR....",
                "................",
                "................"
            },

            // --- 가시밭 길 ---
            new string[]
            {
                "................",
                ".S.S.S.S.S.S.S.S",
                ".S.S.S.S.S.S.S.S",
                ".S.S.S.S.S.S.S.S",
                " S.S.S.S.S.S.S.S", // 좌측 문 통로 확보
                ".S.S.S.S.S.S.S.S",
                ".S.S.S.S.S.S.S.S",
                "................",
                "................"
            },

            // --- 구덩이 미로 ---
            new string[]
            {
                "PPPPPP. ..PPPPPP", // 상단 문 통로 확보
                "P............P.P",
                "P.PPPPPPPPPP.P.P",
                ". P........P.P.P", // 좌측 문 통로 확보
                ". .PPPPPP.P.P.P", // 좌측 문 통로 확보
                ". P.P......P.P.P", // 좌측 문 통로 확보
                "P.P.PPPPPP.P.P.P",
                "P...P........P.P",
                "PPPPPP. ..PPPPPP"  // 하단 문 통로 확보
            },
        };
        #endregion

        #region 생존(Survival) 방 설계도 (문 통로 확보됨)
        private static List<string[]> survivalRoomTemplates = new List<string[]>
        {
            // --- 개방형 ---
            new string[]
            {
                "E...... .. ....E", // 상단 문 통로 확보
                ".R....E.......R.",
                "................",
                "...............E", // 우측 문 통로 확보
                "E..............E", // 우측 문 통로 확보
                "...............E", // 우측 문 통로 확보
                "................",
                ".R....E.......R.",
                "E...... .. ....E"  // 하단 문 통로 확보
            },

            // --- 중앙 아레나 ---
            new string[]
            {
                "E...... .. ......", // 상단 문 통로 확보
                "......E.........",
                "......RRRR......",
                "E....RRRR....E..",
                " ....RRRR.......", // 좌측 문 통로 확보
                "E....RRRR....E..",
                "......RRRR......",
                "........E.......",
                "....... .. ......"  // 하단 문 통로 확보
            },

            // --- 흐르는 강물 ---
            new string[]
            {
                "E...E.. .. ......", // 상단 문 통로 확보
                "..........E...E.",
                "PPPPPPPPPPPPPPPP",
                "................",
                "................",
                "................",
                "PPPPPPPPPPPPPPPP",
                "E...E...........",
                "....... .. ..E.E"  // 하단 문 통로 확보
            }
        };
        #endregion

        #region 보스(Boss) 방 설계도 (문 통로 확보됨)
        private static List<string[]> bossRoomTemplates = new List<string[]>
        {
            // --- 보스 설계도: 원형 경기장 ---
            new string[]
            {
                ".....R. ..RR.....", // 상단 문 통로 확보
                "...R........R...",
                "..R..........R..",
                ".R............R.",
                " R......E.....R ", // 좌우 문 통로 확보
                ".R............R.",
                "..R..........R..",
                "...R........R...",
                ".....R. ..RR....."  // 하단 문 통로 확보
            }
        };
        #endregion

        public static string[] GetLayoutForRoomType(RoomType type)
        {
            List<string[]> selectedTemplateList = null;

            switch (type)
            {
                case RoomType.Puzzle:
                    selectedTemplateList = puzzleRoomTemplates;
                    break;

                case RoomType.Survival:
                    selectedTemplateList = survivalRoomTemplates;
                    break;

                case RoomType.Boss:
                    selectedTemplateList = bossRoomTemplates;
                    break;

                case RoomType.Normal:
                case RoomType.MiniBoss:
                    selectedTemplateList = normalRoomTemplates;
                    break;

                default:
                    return null; // Start, Shop, Treasure 등은 설계도를 사용하지 않음
            }

            if (selectedTemplateList == null || selectedTemplateList.Count == 0)
                return null;

            int index = _random.Next(selectedTemplateList.Count);
            return selectedTemplateList[index];
        }
    }
}