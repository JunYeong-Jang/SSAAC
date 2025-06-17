// 파일 위치: Project_SSAAC/World/Layouts/RoomLayouts.cs
using System;
using System.Collections.Generic;
using Project_SSAAC.World; // RoomType을 사용하기 위해 추가

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

        #region 일반 방(Normal, MiniBoss) 설계도
        private static List<string[]> normalRoomTemplates = new List<string[]>
        {
            // --- 감옥 모양 (적 개수 증가) ---
            new string[]
            {
                "R..............R",
                "R.R.R.R.R.R.R.RR",
                "R...E........E.R",
                "R..............R",
                "R......E.......R",
                "R..............R",
                "RR.R.R.R.R.R.R.R",
                "R...E........E.R",
                "R..............R"
            },

            // --- 네 개의 기둥 (적 개수 증가) ---
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

            // --- 중앙 십자 벽 (적 개수 증가) ---
            new string[]
            {
                "...E............",
                ".......RR.......",
                ".......RR.......",
                "RR...RRRR...RR..",
                "RR.E.RRRR.E.RR..",
                ".......RR.......",
                ".......RR.......",
                "............E...",
                "................"
            },

            // --- 외곽 순찰로 (적 개수 증가) ---
            new string[]
            {
                "RRRRRRRRRRRRRRRR",
                "R.E............R",
                "R.RRRRRRRRRRRR.R",
                "P.P..........P.P",
                "P.P.E....E...P.P",
                "P.P..........P.P",
                "R.RRRRRRRRRRRR.R",
                "R............E.R",
                "RRRRRRRRRRRRRRRR"
            },
        };
        #endregion

        #region 퍼즐(Puzzle) 방 설계도 (적 없음)
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
                ".S.S.S.S.S.S.S.S",
                ".S.S.S.S.S.S.S.S",
                ".S.S.S.S.S.S.S.S",
                "................",
                "................"
            },

            // --- 구덩이 미로 ---
            new string[]
            {
                "PPPPPPPPPPPPPP.P",
                "P............P.P",
                "P.PPPPPPPPPP.P.P",
                "P.P........P.P.P",
                "P.P.PPPPPP.P.P.P",
                "P.P.P......P.P.P",
                "P.P.PPPPPP.P.P.P",
                "P...P........P.P",
                "PPPPPPPPPPPPPP.P"
            },
        };
        #endregion

        #region 생존(Survival) 방 설계도
        private static List<string[]> survivalRoomTemplates = new List<string[]>
        {
            // --- 개방형 (적 개수 대폭 증가) ---
            new string[]
            {
                "E..............E",
                ".R....E.......R.",
                "................",
                "................",
                "E..............E",
                "................",
                "................",
                ".R....E.......R.",
                "E..............E"
            },

            // --- 중앙 아레나 (적 개수 대폭 증가) ---
            new string[]
            {
                "E...............",
                "......E.........",
                "......RRRR......",
                "E....RRRR....E..",
                ".....RRRR.......",
                "E....RRRR....E..",
                "......RRRR......",
                "........E.......",
                "...............E"
            },

            // --- 흐르는 강물 (적 개수 대폭 증가) ---
            new string[]
            {
                "E...E...........",
                "..........E...E.",
                "PPPPPPPPPPPPPPPP",
                "................",
                "................",
                "................",
                "PPPPPPPPPPPPPPPP",
                "E...E...........",
                "..........E...E."
            }
        };
        #endregion

        #region 보스(Boss) 방 설계도
        private static List<string[]> bossRoomTemplates = new List<string[]>
        {
            // --- 보스 설계도: 원형 경기장 (보스 + 부하 소환 느낌으로 적 추가) ---
             new string[]
            {
                ".....RRRRRR.....",
                "...R........R...",
                "..R..........R..",
                ".R............R.",
                ".R......E.....R.", // 중앙 보스 위치만 남김
                ".R............R.",
                "..R..........R..",
                "...R........R...",
                ".....RRRRRR....."
            }
        };
        #endregion

        /// <summary>
        /// 방 타입에 맞는 무작위 설계도를 반환하는 메서드
        /// </summary>
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