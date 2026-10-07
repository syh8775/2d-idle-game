"""
캐릭터 모집 시뮬레이션 (기획서 01 · 01-3)
- 시드: 20261007 / 모집 횟수: 2,000,000회 (10회 모집 200,000번)
- 규칙: ★5 2.0% · ★4 13.0% · ★3 85.0%
        천장: ★5 없이 79회가 지나면 80회째 ★5 확정 (★5가 나오면 카운트 0)
        10회 보장: 10회 결과에 ★4 이상이 없으면 마지막 1개를 ★4로 변경
        (10회 보장으로 바뀐 결과도 천장 카운트에는 일반 결과와 똑같이 1회로 계산)
- 집계 기준
  · 획득률: 보장 적용 후 최종 결과 기준
  · ★5 간격: 직전 ★5 다음 모집부터 다음 ★5까지의 모집 횟수
  · 7장 수집: 특정 캐릭터 1명을 7장(최초 1 + 중복 6) 모으기까지의 모집 횟수, 등급별 대표 1명 기준
  · 등급 구성: Characters.csv 기준 ★5(SSR) 1명 · ★4(SR) 3명 · ★3(R) 5명
"""
import random, json, statistics

SEED, BATCHES = 20261007, 200_000
P5, P4, PITY = 0.02, 0.13, 80
CHARS = {5: ["연화"], 4: ["베로니카", "오데트", "마리엘"], 3: ["아일라", "에델린", "레아", "릴리아", "세실리아"]}  # Characters.csv rarity: SSR / SR / R

def run(guarantee=True, pity=True):
    rnd = random.Random(SEED)
    since, gaps, results, pity_hits, no4_batches = 0, [], [], 0, 0
    for _ in range(BATCHES):
        batch = []
        for _ in range(10):
            since += 1
            if pity and since >= PITY:
                g = 5; pity_hits += 1
            else:
                r = rnd.random()
                g = 5 if r < P5 else (4 if r < P5 + P4 else 3)
            if g == 5:
                gaps.append(since); since = 0
            batch.append(g)
        if max(batch) < 4:
            no4_batches += 1
            if guarantee: batch[-1] = 4
        results.extend(batch)
    # 캐릭터 배정 (같은 등급 안에서 균등)
    rnd2 = random.Random(SEED + 1)
    named = [rnd2.choice(CHARS[g]) for g in results]
    return results, named, gaps, pity_hits, no4_batches

def copies7(named, name):
    """같은 캐릭터 7장을 모을 때마다 걸린 모집 횟수"""
    out, cnt, start = [], 0, 0
    for i, n in enumerate(named):
        if n == name:
            cnt += 1
            if cnt == 7:
                out.append(i + 1 - start); cnt = 0; start = i + 1
    return statistics.mean(out)

res, named, gaps, pity_hits, no4 = run(True, True)
base, _, _, _, _ = run(False, False)
N = len(res)
out = {
    "base_rate5": base.count(5) / N, "base_rate4": base.count(4) / N,
    "rate5": res.count(5) / N, "rate4": res.count(4) / N, "rate3": res.count(3) / N,
    "avg_gap": statistics.mean(gaps), "median": statistics.median(gaps), "maxgap": max(gaps),
    "pity_share": pity_hits / len(gaps), "ten_noSR": no4 / BATCHES,
    "exp_formula": (1 - (1 - P5) ** PITY) / P5,
    "copies7": {g: copies7(named, CHARS[g][0]) for g in (3, 4, 5)},
    "dist": {},
    "p_within45": sum(1 for g in gaps if g <= 45) / len(gaps),
}
for g in gaps:
    k = (g - 1) // 10; key = f"{k*10+1}-{k*10+10}"; out["dist"][key] = out["dist"].get(key, 0) + 1
out["dist"] = {k: v / len(gaps) for k, v in sorted(out["dist"].items(), key=lambda kv: int(kv[0].split("-")[0]))}
print(json.dumps(out, indent=1, ensure_ascii=False))
json.dump(out, open("sim2.json", "w"))
