window.BENCHMARK_DATA = {
  "lastUpdate": 1739616756480,
  "repoUrl": "https://github.com/afxres/binary",
  "entries": {
    "Benchmark": [
      {
        "commit": {
          "author": {
            "email": "stdarg@outlook.com",
            "name": "miko",
            "username": "afxres"
          },
          "committer": {
            "email": "stdarg@outlook.com",
            "name": "miko",
            "username": "afxres"
          },
          "distinct": true,
          "id": "030086a1a84c9980af67587c77afe6149d650cae",
          "message": "Fix benchmark result titles",
          "timestamp": "2025-02-15T18:44:08+08:00",
          "tree_id": "7a374c961c3137c4ba6053998512a7eaff1cb6b0",
          "url": "https://github.com/afxres/binary/commit/030086a1a84c9980af67587c77afe6149d650cae"
        },
        "date": 1739616756163,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "'Encode Named Object (jit)'",
            "value": 97.88888919353485,
            "unit": "ns",
            "range": "± 1.5584630429391342"
          },
          {
            "name": "'Encode Tuple Object (jit)'",
            "value": 57.89118504524231,
            "unit": "ns",
            "range": "± 0.43818658446173286"
          },
          {
            "name": "'Encode Named Object (aot)'",
            "value": 84.66510260105133,
            "unit": "ns",
            "range": "± 0.4439841087033703"
          },
          {
            "name": "'Encode Tuple Object (aot)'",
            "value": 55.0846383968989,
            "unit": "ns",
            "range": "± 0.14127960097036363"
          },
          {
            "name": "'Decode Named Object (jit)'",
            "value": 230.97716649373373,
            "unit": "ns",
            "range": "± 1.4774768841487396"
          },
          {
            "name": "'Decode Tuple Object (jit)'",
            "value": 144.6410862604777,
            "unit": "ns",
            "range": "± 1.3998964793395592"
          },
          {
            "name": "'Decode Named Object (aot)'",
            "value": 210.59222070376077,
            "unit": "ns",
            "range": "± 1.0202558887057913"
          },
          {
            "name": "'Decode Tuple Object (aot)'",
            "value": 144.38756227493286,
            "unit": "ns",
            "range": "± 0.3124106015994536"
          }
        ]
      }
    ]
  }
}