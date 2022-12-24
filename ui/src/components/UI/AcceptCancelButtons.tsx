import React from "react";
import { ButtonGroup, Button } from ".";

export interface Props {
  onAccept?: () => void;
  onCancel?: () => void;
  acceptTitle?: string;
  cancelTitle?: string;
  acceptClassName?: string;
  cancelClassName?: string;
}

export const AcceptCancelButtons = ({
  onAccept,
  onCancel,
  acceptClassName,
  cancelClassName,
  acceptTitle,
  cancelTitle
}: Props) => {
  return (
    <ButtonGroup>
      {acceptTitle && (
        <Button onClick={onAccept} className={acceptClassName}>
          {acceptTitle}
        </Button>
      )}
      {cancelTitle && (
        <Button onClick={onCancel} className={cancelClassName}>
          {cancelTitle}
        </Button>
      )}
    </ButtonGroup>
  );
};
