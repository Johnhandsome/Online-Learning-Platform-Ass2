document.addEventListener("DOMContentLoaded", () => {

  const root = document.getElementById("learn-root");
  if (!root) return;

  const enrollmentId = root.dataset.enrollmentId;

  const btnSummary = document.getElementById("btn-summary");
  const btnAsk = document.getElementById("btn-ask");

  if (btnSummary) {
    btnSummary.addEventListener("click", async () => {
      const lessonId = btnSummary.dataset.lessonId;
      const summaryEl = document.getElementById("ai-summary");

      summaryEl.innerText = "Đợi xíu...";

      try {
        const res = await fetch("/Course/AiSummary", {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            enrollmentId: enrollmentId,
            lessonId: lessonId
          })
        });

        if (!res.ok) {
          summaryEl.innerText = "Không lấy được tóm tắt gòi";
          return;
        }

        summaryEl.innerText = await res.text();
      } catch {
        summaryEl.innerText = "Lỗi";
      }
    });
  }

  if (btnAsk) {
    btnAsk.addEventListener("click", async () => {
      const lessonId = btnAsk.dataset.lessonId;
      const questionInput = document.getElementById("ai-question");
      const answerEl = document.getElementById("ai-answer");

      const question = questionInput.value.trim();
      if (!question) return;

      answerEl.innerText = "Đợi xí đang tìm trả lời cho nè...";

      try {
        const res = await fetch("/Course/AiAsk", {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            enrollmentId: enrollmentId,
            lessonId: lessonId,
            question: question
          })
        });

        if (!res.ok) {
          answerEl.innerText = "Không trả lời được";
          return;
        }

        answerEl.innerText = await res.text();
      } catch {
        answerEl.innerText = "Lỗi";
      }
    });
  }
});
