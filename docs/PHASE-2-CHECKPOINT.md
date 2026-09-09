# 阶段 2 续作检查点

日期：2026-09-09  
状态：已恢复制作；当前因最终 v2 确定性验证失败而暂停，阶段 2 尚未完成。

## 已完成并通过检查

- 主参考图 `base`
- `idle`：6 帧
- `running-right`：8 帧
- `running-left`：8 帧；因向日葵发饰和服装不对称，单独生成而非镜像
- `waving`：4 帧
- `jumping`：5 帧

所有上述动作均已复制到本地运行目录并通过对应的逐帧结构检查。

## 已完成并通过检查（续）

- `failed`：重新生成紧凑连通版本，使用经视觉确认的 `stable-slots` 拆帧
- `waiting`、非移动 `running`：已生成，使用经视觉确认的 `stable-slots` 拆帧
- `review`：已生成并通过组件拆帧
- 标准 8×9 图集、联系表和 9 个 GIF：结构及独立视觉 QA 通过
- 四方向锚点：`000` 上、`090` 屏幕右、`180` 下、`270` 屏幕左均通过
- `look-row-9`：首版因姿势相连失败，第二版因 `000`/`157.5` 语义失败，修复版通过配准与独立语义 QA
- `look-row-10`：通过装配与完整 16 向标注语义 QA
- 完整图集尺寸：`1536×2288`，8×11、每格 192×208

## 当前停点（最终验证故障）

最终唯一一次 `despill_chroma_edges.py` 已执行并返回 `ok: true`，保持 alpha 且无拒绝像素。随后 `validate_atlas.py --require-v2` 返回 `ok: false`：

```text
running-right row 1 column 7 has 5 visible edge pixels contaminated by chroma key #FF00FF
```

按 `hatch-pet` 规则，去品红成功后不得再次执行清色、调阈值或通过重生成图像掩盖验证失败；因此流水线在盲测、正式打包和安装前停止。

关键证据：

```text
artifacts/pet-runs/fengjin-v2/qa/chroma-despill-extended.json
artifacts/pet-runs/fengjin-v2/final/validation-extended.json
artifacts/pet-runs/fengjin-v2/final/spritesheet-extended.png
artifacts/pet-runs/fengjin-v2/final/spritesheet-extended.webp
artifacts/pet-runs/fengjin-v2/qa/contact-sheet-extended.png
artifacts/pet-runs/fengjin-v2/qa/look-directions.png
artifacts/pet-runs/fengjin-v2/qa/direction-semantics.json
artifacts/pet-runs/fengjin-v2/qa/look-continuity.json
```

## 旧停点（已解决）

`failed` 已生成 8 个可见姿势，但自动拆帧和 `stable-slots` 拆帧都把第 4 帧误判为空帧。源图视觉上确实存在 8 个完整姿势，因此当前先归类为拆帧/槽位配准问题，不将该任务标记为完成。

证据文件位于：

```text
artifacts/pet-runs/fengjin-v2/decoded/failed.png
artifacts/pet-runs/fengjin-v2/qa/rows/failed/review.json
```

`artifacts/` 是可再生成的本地工作区，受 `.gitignore` 排除，不上传 GitHub。

## 下次从这里继续

1. 诊断“去品红报告成功但 WebP v2 验证残留 5 个边缘品红像素”的确定性流水线矛盾；不要直接重跑清色。
2. 在符合 `hatch-pet` 规则的修复路径明确后，从未清色装配步骤重新建立一个可审计的新运行版本。
3. 通过 v2 验证后再生成并完成三个隔离盲测、最终视觉 QA、打包和安装。
4. 将最终素材复制到项目 `assets/` 与 `08_Exports/codex-usage-pet`，更新阶段报告并推送。

不要重新生成已经通过语义与动画 QA 的角色动作或视线行，除非确定性诊断证明源图本身是唯一根因。
