# 阶段 2 续作检查点

日期：2026-09-09  
状态：因当日用量不足主动暂停；阶段 2 尚未完成。

## 已完成并通过检查

- 主参考图 `base`
- `idle`：6 帧
- `running-right`：8 帧
- `running-left`：8 帧；因向日葵发饰和服装不对称，单独生成而非镜像
- `waving`：4 帧
- `jumping`：5 帧

所有上述动作均已复制到本地运行目录并通过对应的逐帧结构检查。

## 当前停点

`failed` 已生成 8 个可见姿势，但自动拆帧和 `stable-slots` 拆帧都把第 4 帧误判为空帧。源图视觉上确实存在 8 个完整姿势，因此当前先归类为拆帧/槽位配准问题，不将该任务标记为完成。

证据文件位于：

```text
artifacts/pet-runs/fengjin-v2/decoded/failed.png
artifacts/pet-runs/fengjin-v2/qa/rows/failed/review.json
```

`artifacts/` 是可再生成的本地工作区，受 `.gitignore` 排除，不上传 GitHub。

## 下次从这里继续

1. 检查 `failed.png` 的姿势中心分布与拆帧器槽位推断；若不能确定性修复，则只重新生成完整 `failed` 行。
2. 生成并逐行检查 `waiting`、`running`、`review`。
3. 组装并视觉检查标准 8×9 图集和各行动画预览。
4. 编写 `qa/look-mechanics.md`，再生成四方向锚点以及两行 16 向视线。
5. 完成 v2 图集、盲测、连续性、透明度和最终视觉 QA，然后打包、接入项目并推送。

不要重新生成已经完成的 `base`、`idle`、左右移动、`waving` 或 `jumping`。
