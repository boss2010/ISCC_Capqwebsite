import json
from collections import Counter

with open(r"tmp\compare_payments\excel_rows.json", encoding="utf-8") as f:
    excel_rows = json.load(f)
with open(r"tmp\compare_payments\pdf_extract.json", encoding="utf-8") as f:
    pdf = json.load(f)

excel = []
for idx, row in enumerate(excel_rows, start=4):
    if len(row) > 1 and row[1] not in (None, ""):
        excel.append({"row": idx, "transaction": row[0], "order_id": str(row[1]).strip().lower(), "date": row[3], "name": row[4], "office": row[6], "details": row[12], "amount": row[13]})

pdf_orders = pdf["orders"]
ec = Counter(x["order_id"] for x in excel)
pc = Counter(x["order_id"] for x in pdf_orders)
only_pdf = []
only_excel = []
for oid in sorted(set(pc) | set(ec)):
    if pc[oid] > ec[oid]:
        only_pdf.extend([x for x in pdf_orders if x["order_id"] == oid][:pc[oid]-ec[oid]])
    if ec[oid] > pc[oid]:
        only_excel.extend([x for x in excel if x["order_id"] == oid][:ec[oid]-pc[oid]])

amount_mismatches = []
for oid in sorted(set(pc) & set(ec)):
    es = [x for x in excel if x["order_id"] == oid]
    ps = [x for x in pdf_orders if x["order_id"] == oid]
    if len(es) == len(ps):
        for e, p in zip(es, ps):
            if abs(float(e["amount"]) - float(p["amount"])) > 0.009:
                amount_mismatches.append({"order_id": oid, "excel": e, "pdf": p})

result = {"excel_count": len(excel), "excel_unique": len(ec), "pdf_count": len(pdf_orders), "pdf_unique": len(pc), "only_pdf": only_pdf, "only_excel": only_excel, "amount_mismatches": amount_mismatches, "excel_duplicates": {k:v for k,v in ec.items() if v>1}, "pdf_duplicates": {k:v for k,v in pc.items() if v>1}}
with open(r"tmp\compare_payments\comparison.json", "w", encoding="utf-8") as f:
    json.dump(result, f, ensure_ascii=False, indent=2)
print(json.dumps(result, ensure_ascii=True, indent=2))
