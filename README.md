# RAGSharp


Input Text
    ↓
Split by paragraphs (\n\n)
    ↓
For each paragraph:
    ├─ Fits in chunk size? → Yield whole paragraph
    └─ Too large? → Split by sentences
                      ↓
                For each sentence:
                    ├─ Buffer + sentence fits? → Add to buffer
                    └─ Too large?
                         ├─ Yield buffer
                         └─ Single sentence too large? → Token window split
                                                       └─ Use sliding window with overlap